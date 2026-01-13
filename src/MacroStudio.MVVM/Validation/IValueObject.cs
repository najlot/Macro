using System.Collections.Generic;

namespace MacroStudio.MVVM.Validation;

public interface IValueObject
{
	IEnumerable<ValidationResult> Validate();
}