using System;
using System.ComponentModel.DataAnnotations;


//性别Validation
public class GenderValidationAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success;
        }

        var gender = value as string;
        if (gender == "男" || gender == "女" || gender == "未知" || gender == "其他")
        {
            return ValidationResult.Success;
        }

        return new ValidationResult("性别必须是[男 女 未知 其他 ]中的一个。");
    }
}
