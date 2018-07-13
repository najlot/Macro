
#include <Windows.h>

#define LISTENER_SLEEP_TIME 5

using System::Threading::Thread;
using System::Collections::Generic::List;
using System::Diagnostics::Stopwatch;
using System::String;
using System::Threading::ThreadStart;

public ref class TimedAction
{
public:
	int Key;
	int X;
	int Y;
	bool keyDown;
	INT64 ms;

	void ReadFromLine(String ^line)
	{
		array<String^> ^stringArray;
		stringArray = line->Split(';');

		Key = System::Convert::ToInt32(stringArray[0], 10);
		X = System::Convert::ToInt32(stringArray[1], 10);
		Y = System::Convert::ToInt32(stringArray[2], 10);

		if (stringArray[3] == L"1")
		{
			keyDown = true;
		}
		else
		{
			keyDown = false;
		}

		ms = System::Convert::ToInt64(stringArray[4], 10);
	}

	String^ GetAsLine()
	{
		String ^line = gcnew String(L"");

		if (keyDown)
		{
			line = line->Concat(Key.ToString(), L";", X.ToString(), L";", Y.ToString(), L";1;", ms.ToString(), L"\n");
		}
		else
		{
			line = line->Concat(Key.ToString(), L";", X.ToString(), L";", Y.ToString(), L";0;", ms.ToString(), L"\n");
		}

		return line;
	}

};

void ListenerRegisterKeyAction(int key, bool down);

public ref class KeyUpListener
{
private:
	bool stop = false;
	int WaitingKey = 0;

public:

	KeyUpListener(int key)
	{
		WaitingKey = key;
	}

	void Start()
	{
		stop = false;

		while (!stop)
		{
			Sleep(LISTENER_SLEEP_TIME);

			if (!(GetAsyncKeyState(WaitingKey) & 0x8000))
			{
				ListenerRegisterKeyAction(WaitingKey, false);
				return;
			}
		}
	}

	void Stop()
	{
		stop = true;
	}

};

public ref class KeyListener
{
private:
	static List<bool> KeysDown;
	static List<KeyUpListener^> KeyUpListeners;
	static List<Thread^> KeyUpListenerThreads;

	static List<TimedAction^> TimedActions;
	static Stopwatch stopwatch;

public:

	static KeyListener()
	{
		for (int i = 0; i < 256; i++)
		{
			KeysDown.Add(false);
			KeyUpListeners.Add(gcnew KeyUpListener(i));
			KeyUpListenerThreads.Add(gcnew Thread(gcnew ThreadStart(KeyUpListeners[i], &KeyUpListener::Start)));
		}

		stopwatch.Start();
	}

	static void RegisterKeyAction(int key, bool down)
	{
		POINT pCursorPos;
		TimedAction ^ timedAction = gcnew TimedAction();

		KeysDown[key] = down;
		GetCursorPos(&pCursorPos);

		timedAction->Key = key;
		timedAction->X = pCursorPos.x;
		timedAction->Y = pCursorPos.y;
		timedAction->keyDown = down;
		timedAction->ms = stopwatch.ElapsedMilliseconds;

		/*
		System::Console::Write("- ");
		System::Console::Write(timedAction->ms.ToString());
		System::Console::Write(" - ");
		System::Console::Write(key.ToString());
		System::Console::Write(" - ");
		System::Console::Write(down.ToString());
		System::Console::WriteLine();
		*/

		TimedActions.Add(timedAction);

		stopwatch.Reset();
		stopwatch.Start();
	}
	
	int WaitForKeyDown()
	{
		while (true)
			for (int i = 1; i < 256; i++)
			{
				if (GetAsyncKeyState(i) & 0x8000)
				{
					if (stopwatch.ElapsedMilliseconds < 25) continue; // So schnell kann keiner klicken...

					if (i == 27)
					{
						StopAllListenerThreads();
						return i;
					}

					if (KeysDown[i]) continue;
					RegisterKeyAction(i, true);

					KeyUpListeners[i]->Stop();
					if (KeyUpListenerThreads[i]->IsAlive)KeyUpListenerThreads[i]->Join();

					KeyUpListenerThreads[i] = gcnew Thread(gcnew ThreadStart(KeyUpListeners[i], &KeyUpListener::Start));

					KeyUpListenerThreads[i]->Start();
				}
			}

		return 0;
	}

	void WriteActionsToFile(System::String^ path)
	{
		String ^ line;
		System::IO::FileStream ^fstream;
		System::Text::UTF8Encoding ^encoding = gcnew System::Text::UTF8Encoding();

		if (System::IO::File::Exists(path))
		{
			System::IO::File::Delete(path);
		}

		fstream = System::IO::File::OpenWrite(path);
		
		for each (TimedAction^ timedAction in TimedActions)
		{
			line = timedAction->GetAsLine();
			fstream->Write(encoding->GetBytes(line), 0, encoding->GetByteCount(line));
		}

		fstream->Close();
	}

	void StopAllListenerThreads()
	{
		for each (KeyUpListener^ keyUpListener in KeyUpListeners)
		{
			keyUpListener->Stop();
		}
	}

};

void ListenerRegisterKeyAction(int key, bool down)
{
	KeyListener ^keyListener = gcnew KeyListener();
	keyListener->RegisterKeyAction(key, down);
	keyListener = nullptr;
}

[System::STAThread]
int main()
{
	System::Console::WriteLine(L"Aufnahme gestartet!");

	KeyListener ^keyListener = gcnew KeyListener();
	System::Windows::Forms::SaveFileDialog ^dlg;

	try
	{
		HWND handle = GetConsoleWindow();
		ShowWindow(handle, SW_HIDE);

		keyListener->WaitForKeyDown();
		keyListener->StopAllListenerThreads();

		dlg = gcnew System::Windows::Forms::SaveFileDialog();

		dlg->Filter = L"Macro files (*.MACRO)|*.MACRO";
		dlg->FilterIndex = 1;

		if (dlg->ShowDialog() == System::Windows::Forms::DialogResult::OK)
		{
			keyListener->WriteActionsToFile(dlg->FileName);
		}
	}
	catch (System::Exception ^e)
	{
		System::Console::WriteLine(e->ToString());
		System::Windows::Forms::MessageBox::Show(e->ToString());
	}
}
