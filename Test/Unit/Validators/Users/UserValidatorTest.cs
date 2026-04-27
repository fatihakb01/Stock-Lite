using Application.Entities.Core.Users.DTOs;
using Application.Entities.Core.Users.Validators;
using Domain.Enums;
using FluentValidation;
using FluentValidation.TestHelper;
using System.Linq.Expressions;

namespace Test.Unit.Validators.Users
{
    public class UserValidatorTest
    {
        // -------------------------------------------------------
        // Helpers — DTOs
        // -------------------------------------------------------
        private static RegisterUserDto ValidRegisterDto() => new RegisterUserDto()
        {
            UserName = "testuser",
            Email = "test@test.com",
            Password = "Password1!",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "+31612345678",
            Role = UserRole.Admin
        };

        private static EditUserDto ValidEditDto() => new EditUserDto()
        {
            UserName = "testuser",
            Email = "test@test.com",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "+31612345678",
            Role = UserRole.Admin
        };

        private static LoginUserDto ValidLoginDto() => new LoginUserDto() 
        { 
            Email = "test@test.com", 
            Password = "Password1!" 
        };
        private static ForgotPasswordDto ValidForgotPasswordDto() => new ForgotPasswordDto() 
        { 
            UserId = Guid.NewGuid() 
        };
        private static ChangePasswordDto ValidChangePasswordDto() => new ChangePasswordDto() 
        { 
            UserId = Guid.NewGuid(), 
            Token = "valid-token", 
            NewPassword = "Password1!" 
        };
        private static ConfirmEmailDto ValidConfirmEmailDto() => new ConfirmEmailDto() 
        { 
            UserId = Guid.NewGuid(), 
            Token = "valid-token" 
        };

        // -------------------------------------------------------
        // Shared rule testers
        // -------------------------------------------------------

        private static void AssertPasswordRules<T>(
            IValidator<T> validator,
            T validDto,
            Action<T, string> setPassword,
            Expression<Func<T, object>> passwordSelector)
        {
            void Test(string password, string? expectedMessage = null)
            {
                setPassword(validDto, password);
                var result = validator.TestValidate(validDto);
                var assertion = result.ShouldHaveValidationErrorFor(passwordSelector);
                if (expectedMessage != null)
                    assertion.WithErrorMessage(expectedMessage);
            }

            Test("", null);
            Test("Ab1!", null);
            Test("password1!", "Password must contain at least one uppercase letter.");
            Test("PASSWORD1!", "Password must contain at least one lowercase letter.");
            Test("Password!", "Password must contain at least one number.");
            Test("Password1", "Password must contain at least one special character.");
        }

        private static void AssertEmailRules<T>(
            IValidator<T> validator,
            T validDto,
            Action<T, string> setEmail,
            Expression<Func<T, object>> emailSelector)
        {
            foreach (var invalid in new[] { "", "notanemail", "missing@", "@nodomain.com" })
            {
                setEmail(validDto, invalid);
                validator.TestValidate(validDto).ShouldHaveValidationErrorFor(emailSelector);
            }
        }

        private static void AssertOptionalMaxLength<T>(
            IValidator<T> validator,
            T validDto,
            Action<T, string?> setField,
            Expression<Func<T, object>> selector,
            int maxLength)
        {
            // null is allowed
            setField(validDto, null);
            validator.TestValidate(validDto).ShouldNotHaveValidationErrorFor(selector);

            // matches max length
            setField(validDto, new string('A', maxLength));
            validator.TestValidate(validDto).ShouldNotHaveValidationErrorFor(selector);

            // exceed max length
            setField(validDto, new string('A', maxLength + 1));
            validator.TestValidate(validDto).ShouldHaveValidationErrorFor(selector);
        }

        private static void AssertPhoneRules<T>(
            IValidator<T> validator,
            T validDto,
            Action<T, string?> setPhone,
            Expression<Func<T, object>> selector)
        {
            // null is allowed
            setPhone(validDto, null);
            validator.TestValidate(validDto).ShouldNotHaveValidationErrorFor(selector);

            // valid formats
            foreach (var valid in new[] { "+31612345678", "0612345678", "+1 (800) 555-1234", "06-12345678" })
            {
                setPhone(validDto, valid);
                validator.TestValidate(validDto).ShouldNotHaveValidationErrorFor(selector);
            }

            // invalid formats
            foreach (var invalid in new[] { "abc", "12", "notaphone!" })
            {
                setPhone(validDto, invalid);
                validator.TestValidate(validDto)
                    .ShouldHaveValidationErrorFor(selector)
                    .WithErrorMessage("Phone number format is invalid.");
            }
        }

        private static void AssertTokenRules<T>(
            IValidator<T> validator,
            T validDto,
            Action<T, string> setToken,
            Expression<Func<T, object>> selector)
        {
            // empty string
            setToken(validDto, "");
            validator.TestValidate(validDto)
                .ShouldHaveValidationErrorFor(selector)
                .WithErrorMessage("Token is required.");

            // exceed max length
            setToken(validDto, new string('A', 501));
            validator.TestValidate(validDto)
                .ShouldHaveValidationErrorFor(selector)
                .WithErrorMessage("Token is too long.");

            // matches max length
            setToken(validDto, new string('A', 500));
            validator.TestValidate(validDto).ShouldNotHaveValidationErrorFor(selector);
        }

        // -------------------------------------------------------
        // RegisterUserValidator
        // -------------------------------------------------------
        [Fact]
        public void Register_UserName_Rules()
        {
            var validator = new RegisterUserValidator();
            var dto = ValidRegisterDto();

            dto.UserName = "";
            validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.UserName);

            dto.UserName = new string('A', 51);
            validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.UserName);

            dto.UserName = new string('A', 50);
            validator.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.UserName);
        }

        [Fact]
        public void Register_Email_Rules()
        {
            var validator = new RegisterUserValidator();
            AssertEmailRules(validator, ValidRegisterDto(), (d, v) => d.Email = v, x => x.Email);
        }

        [Fact]
        public void Register_Password_Rules()
        {
            var validator = new RegisterUserValidator();
            AssertPasswordRules(validator, ValidRegisterDto(), (d, v) => d.Password = v, x => x.Password);
        }

        [Fact]
        public void Register_FirstName_Rules()
        {
            var validator = new RegisterUserValidator();
            AssertOptionalMaxLength(validator, ValidRegisterDto(), (d, v) => d.FirstName = v, x => x.FirstName!, 50);
        }

        [Fact]
        public void Register_LastName_Rules()
        {
            var validator = new RegisterUserValidator();
            AssertOptionalMaxLength(validator, ValidRegisterDto(), (d, v) => d.LastName = v, x => x.LastName!, 50);
        }

        [Fact]
        public void Register_PhoneNumber_Rules()
        {
            var validator = new RegisterUserValidator();
            AssertPhoneRules(validator, ValidRegisterDto(), (d, v) => d.PhoneNumber = v, x => x.PhoneNumber!);
        }

        [Fact]
        public void Register_Role_InvalidValue_ShouldHaveValidationError()
        {
            var validator = new RegisterUserValidator();
            var dto = ValidRegisterDto();
            dto.Role = (UserRole)99;

            validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Role);
        }

        [Fact]
        public void Register_AllFieldsValid_ShouldHaveNoErrors()
        {
            var validator = new RegisterUserValidator();
            validator.TestValidate(ValidRegisterDto()).ShouldNotHaveAnyValidationErrors();
        }

        // -------------------------------------------------------
        // RegisterValidator (EditUserDto)
        // -------------------------------------------------------
        [Fact]
        public void Edit_UserName_Rules()
        {
            var validator = new RegisterValidator();
            var dto = ValidEditDto();

            dto.UserName = "";
            validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.UserName);

            dto.UserName = new string('A', 51);
            validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.UserName);

            dto.UserName = new string('A', 50);
            validator.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.UserName);
        }

        [Fact]
        public void Edit_Email_Rules()
        {
            var validator = new RegisterValidator();
            AssertEmailRules(validator, ValidEditDto(), (d, v) => d.Email = v, x => x.Email);
        }

        [Fact]
        public void Edit_FirstName_Rules()
        {
            var validator = new RegisterValidator();
            AssertOptionalMaxLength(validator, ValidEditDto(), (d, v) => d.FirstName = v, x => x.FirstName!, 50);
        }

        [Fact]
        public void Edit_LastName_Rules()
        {
            var validator = new RegisterValidator();
            AssertOptionalMaxLength(validator, ValidEditDto(), (d, v) => d.LastName = v, x => x.LastName!, 50);
        }

        [Fact]
        public void Edit_PhoneNumber_Rules()
        {
            var validator = new RegisterValidator();
            AssertPhoneRules(validator, ValidEditDto(), (d, v) => d.PhoneNumber = v, x => x.PhoneNumber!);
        }

        [Fact]
        public void Edit_Role_InvalidValue_ShouldHaveValidationError()
        {
            var validator = new RegisterValidator();
            var dto = ValidEditDto();
            dto.Role = (UserRole)99;

            validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Role);
        }

        [Fact]
        public void Edit_AllFieldsValid_ShouldHaveNoErrors()
        {
            var validator = new RegisterValidator();
            validator.TestValidate(ValidEditDto()).ShouldNotHaveAnyValidationErrors();
        }

        // -------------------------------------------------------
        // LoginValidator
        // -------------------------------------------------------
        [Fact]
        public void Login_Email_Rules()
        {
            var validator = new LoginValidator();
            AssertEmailRules(validator, ValidLoginDto(), (d, v) => d.Email = v, x => x.Email);
        }

        [Fact]
        public void Login_Password_Rules()
        {
            var validator = new LoginValidator();
            AssertPasswordRules(validator, ValidLoginDto(), (d, v) => d.Password = v, x => x.Password);
        }

        [Fact]
        public void Login_AllFieldsValid_ShouldHaveNoErrors()
        {
            var validator = new LoginValidator();
            validator.TestValidate(ValidLoginDto()).ShouldNotHaveAnyValidationErrors();
        }

        // -------------------------------------------------------
        // ResetPasswordValidator
        // -------------------------------------------------------
        [Fact]
        public void ResetPassword_UserId_Empty_ShouldHaveValidationError()
        {
            var validator = new ResetPasswordValidator();
            validator.TestValidate(new ForgotPasswordDto { UserId = Guid.Empty })
                .ShouldHaveValidationErrorFor(x => x.UserId);
        }

        [Fact]
        public void ResetPassword_AllFieldsValid_ShouldHaveNoErrors()
        {
            var validator = new ResetPasswordValidator();
            validator.TestValidate(ValidForgotPasswordDto()).ShouldNotHaveAnyValidationErrors();
        }

        // -------------------------------------------------------
        // ChangePasswordValidator
        // -------------------------------------------------------
        [Fact]
        public void ChangePassword_Token_Rules()
        {
            var validator = new ChangePasswordValidator();
            AssertTokenRules(validator, ValidChangePasswordDto(), (d, v) => d.Token = v, x => x.Token);
        }

        [Fact]
        public void ChangePassword_Password_Rules()
        {
            var validator = new ChangePasswordValidator();
            AssertPasswordRules(validator, ValidChangePasswordDto(), (d, v) => d.NewPassword = v, x => x.NewPassword);
        }

        [Fact]
        public void ChangePassword_AllFieldsValid_ShouldHaveNoErrors()
        {
            var validator = new ChangePasswordValidator();
            validator.TestValidate(ValidChangePasswordDto()).ShouldNotHaveAnyValidationErrors();
        }

        // -------------------------------------------------------
        // ConfirmValidator
        // -------------------------------------------------------
        [Fact]
        public void ConfirmEmail_Token_Rules()
        {
            var validator = new ConfirmValidator();
            AssertTokenRules(validator, ValidConfirmEmailDto(), (d, v) => d.Token = v, x => x.Token);
        }

        [Fact]
        public void ConfirmEmail_AllFieldsValid_ShouldHaveNoErrors()
        {
            var validator = new ConfirmValidator();
            validator.TestValidate(ValidConfirmEmailDto()).ShouldNotHaveAnyValidationErrors();
        }
    }
}