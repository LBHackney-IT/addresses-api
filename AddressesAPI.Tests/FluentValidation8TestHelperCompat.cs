using System;
using System.Linq.Expressions;
using FluentValidation;
using FluentValidation.TestHelper;

namespace FluentValidation.TestHelper
{
    /// <summary>
    /// Restores FluentValidation 8 TestHelper APIs on top of the 11.x TestValidate model.
    /// </summary>
    public static class FluentValidation8TestHelperCompat
    {
        public static ITestValidationWith ShouldHaveValidationErrorFor<T, TProperty>(
            this IValidator<T> validator,
            Expression<Func<T, TProperty>> expression,
            T instance)
        {
            return validator.TestValidate(instance).ShouldHaveValidationErrorFor(expression);
        }

        public static void ShouldNotHaveValidationErrorFor<T, TProperty>(
            this IValidator<T> validator,
            Expression<Func<T, TProperty>> expression,
            T instance)
        {
            validator.TestValidate(instance).ShouldNotHaveValidationErrorFor(expression);
        }

        public static TestValidationResult<T> ShouldNotHaveError<T>(this TestValidationResult<T> result)
        {
            result.ShouldNotHaveAnyValidationErrors();
            return result;
        }

        public static ITestValidationContinuation ShouldHaveError<T>(this TestValidationResult<T> result)
        {
            return result.ShouldHaveAnyValidationError();
        }
    }
}
