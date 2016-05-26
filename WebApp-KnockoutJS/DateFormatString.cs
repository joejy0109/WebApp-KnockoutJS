using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JOEJY
{
    public class DateFormatString : ValidationAttribute
    {
        public string Format { get; set; }

        public DateFormatString(string format)
        {
            if (string.IsNullOrWhiteSpace(format))
                throw new ArgumentNullException("format");

            this.Format = format;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value.GetType() == typeof(string) && !string.IsNullOrWhiteSpace(value.ToString()))            
            {
                DateTime datetime;
                if (!DateTime.TryParseExact(value.ToString(), Format, null, System.Globalization.DateTimeStyles.None, out datetime))
                {
                    return new ValidationResult(base.ErrorMessage);
                }
            }
            return ValidationResult.Success;
        }
    }

}
