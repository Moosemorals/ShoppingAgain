using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingAgain.ViewModels
{
    public class ChangePasswordVM : IValidatableObject
    {
        public Guid UserId { get; set; }

        [Required, DataType(DataType.Password)]
        [Display(Name = "Old Password")]
        public string OldPassword { get; set; }

        [Required, DataType(DataType.Password), MinLength(12, ErrorMessage = "Your password must be at least 12 characters long")]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [Required, DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        public string ConfirmPassword { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (NewPassword == OldPassword)
            {
                yield return new ValidationResult(
                    "Your new password must not be the same as the old password",
                    new string[] { nameof(NewPassword) }
                );
            }
            if (NewPassword != ConfirmPassword)
            {
                yield return new ValidationResult(
                    "New password and confrimation didn't match",
                    new[] { nameof(NewPassword), nameof(ConfirmPassword) }
                );
            }
        }
    }
}
