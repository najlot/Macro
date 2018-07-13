
#include <Windows.h>

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

ref class KeyPressSimmulator
{
	static List<TimedAction^> TimedActions;

public:

	void ReadFromFile(String ^path)
	{
		System::String ^ line;
		System::IO::StreamReader ^ str;
		str = System::IO::File::OpenText(path);
		TimedAction ^action;

		while (line = str->ReadLine())
		{
			if (line->Length == 0) continue;
			action = gcnew TimedAction();
			action->ReadFromLine(line);
			TimedActions.Add(action);
		}

		str->Close();
	}

	void Execute()
	{
		Stopwatch stopwatch;
		int posX, posY;
		double percent;
		POINT pCursorPos;

		for each (TimedAction^ action in TimedActions)
		{
			stopwatch.Reset();
			stopwatch.Start();

			while (stopwatch.ElapsedMilliseconds < action->ms)
			{
				if (GetAsyncKeyState(27) & 0x8000)return;

				System::Threading::Thread::Sleep(100);
				
				percent = ((double)stopwatch.ElapsedMilliseconds) / ((double)action->ms);

				GetCursorPos(&pCursorPos);

				posX = pCursorPos.x + (action->X - pCursorPos.x) * percent;
				posY = pCursorPos.y + (action->Y - pCursorPos.y) * percent;

				SetCursorPos(posX, posY);
			}
			
			SetCursorPos(action->X, action->Y);

			if (action->Key == VK_LBUTTON)
			{
				if (action->keyDown)
				{
					mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
				}
				else
				{
					mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
				}
			}
			else if (action->Key == VK_RBUTTON)
			{
				if (action->keyDown)
				{
					mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
				}
				else
				{
					mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
				}
			}
			else if (action->Key == VK_MBUTTON)
			{
				if (action->keyDown)
				{
					mouse_event(MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, 0);
				}
				else
				{
					mouse_event(MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0);
				}
			}
			else
			{
				if (action->keyDown)
				{
					keybd_event(action->Key, action->Key, 0, 0);
				}
				else
				{
					keybd_event(action->Key, action->Key, KEYEVENTF_KEYUP, 0);
				}
			}

		}
	}

};

[System::STAThread]
int main(array<String^>^ args)
{
	KeyPressSimmulator ^ keyPressSimmulator = gcnew KeyPressSimmulator();
	System::Windows::Forms::OpenFileDialog ^dlg;
	dlg = gcnew System::Windows::Forms::OpenFileDialog();

	dlg->Filter = L"Macro files (*.MACRO)|*.MACRO";
	dlg->FilterIndex = 1;

	try
	{
		HWND handle = GetConsoleWindow();
		ShowWindow(handle, SW_HIDE);

		if (args->Length == 0)
		{
			if (dlg->ShowDialog() == System::Windows::Forms::DialogResult::OK)
			{
				keyPressSimmulator->ReadFromFile(dlg->FileName);
			}
		}
		else
		{
			keyPressSimmulator->ReadFromFile(gcnew System::String(args[0]));
		}

		keyPressSimmulator->Execute();
	}
	catch (System::Exception ^ e)
	{
		System::Console::WriteLine(e->ToString());
		System::Windows::Forms::MessageBox::Show(e->ToString());
	}

	
}
