using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Pos.Helpers
{
    public static class ValidationHelper
    {
        public static void ValidateModel<T>(T model, bool throwIfInvalid = true)
        {
            if (model == null)
                throw new ArgumentNullException("model", "Модель не может быть пустой");

            var context = new ValidationContext(model, null, null);
            var results = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(model, context, results, true);

            if (!isValid && throwIfInvalid)
            {
                string errorMessage = string.Join("; ", results.Select(r => r.ErrorMessage));
                throw new ValidationException(errorMessage);
            }
        }

        public static List<string> GetErrors<T>(T model)
        {
            if (model == null)
                return new List<string> { "Модель не может быть пустой" };

            var context = new ValidationContext(model, null, null);
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(model, context, results, true);

            return results.Select(r => r.ErrorMessage).ToList();
        }
    }
}
