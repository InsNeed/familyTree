using System.ComponentModel.DataAnnotations;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Linq;

//判断图片合法性
public class PhotoValidationAttribute : ValidationAttribute
{
	private readonly string[] _extensions;
	public PhotoValidationAttribute(string[] extensions)
	{
		_extensions = extensions;
	}

	protected override ValidationResult IsValid(object value, ValidationContext validationContext)
	{
		var file = value as IFormFile;
		if (file != null)
		{
			var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
			if (!_extensions.Contains(extension))
			{
				return new ValidationResult($"This photo extension is not allowed!");
			}
		}

		return ValidationResult.Success;
	}
}
