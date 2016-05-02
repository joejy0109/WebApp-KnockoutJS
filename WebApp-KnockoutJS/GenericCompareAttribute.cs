using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace System.ComponentModel.DataAnnotations
{
    public enum GenericCompareOperator
    {
        GreatherThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual
    }
    public class GenericCompareAttribute : ValidationAttribute, IClientValidatable
    {
        private GenericCompareOperator _operatorName = GenericCompareOperator.GreaterThanOrEqual;

        public string CompareToPropertyName { get; set; }

        public GenericCompareOperator OperatorName
        {
            get { return _operatorName; }
            set { _operatorName = value; }
        }

        public GenericCompareAttribute() : base()        
        {

        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string operString;
            switch(OperatorName)
            {
                case GenericCompareOperator.GreatherThan:
                    operString = "greater than ";
                    break;                
                case GenericCompareOperator.LessThan:
                    operString = "greater than ";
                    break;
                case GenericCompareOperator.LessThanOrEqual:
                    operString = "less than or equal to ";
                    break;
                default:
                    operString = "greater than or equal to ";
                    break;
            }

            var basePropertyInfo = validationContext.ObjectType.GetProperty(CompareToPropertyName);
            var valOther = (IComparable)basePropertyInfo.GetValue(validationContext.ObjectInstance, null);
            var valThis = (IComparable)value;

            if ((OperatorName == GenericCompareOperator.GreatherThan && valThis.CompareTo(valOther) <= 0) || 
                (OperatorName == GenericCompareOperator.GreaterThanOrEqual && valThis.CompareTo(valOther) < 0) || 
                (OperatorName == GenericCompareOperator.LessThan && valThis.CompareTo(valOther) >= 0) || 
                (OperatorName == GenericCompareOperator.LessThanOrEqual && valThis.CompareTo(valOther) > 0))
            {
                return new ValidationResult(base.ErrorMessage);
            }
            return null;            
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var errorMessage = this.FormatErrorMessage(metadata.DisplayName);
            var compareRule = new ModelClientValidationRule();
            compareRule.ErrorMessage = errorMessage;
            compareRule.ValidationType = "genericcompare";
            compareRule.ValidationParameters.Add("comparetopropertyname", CompareToPropertyName);
            compareRule.ValidationParameters.Add("operatorname", OperatorName.ToString());
            yield return compareRule;
        }
    }
}
