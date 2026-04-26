using Application.Entities.Core.ExportProfiles.DTOs;
using Application.Entities.Core.ExportProfiles.Validators;
using Domain.Enums;
using FluentValidation.TestHelper;

namespace Test.Unit.Validators.Export
{
    public class ExportProfileValidatorTest
    {
        private readonly BaseExportProfileDtoValidator _validator = new BaseExportProfileDtoValidator();

        // -------------------------------------------------------
        // Helpers
        // -------------------------------------------------------
        private static BaseExportProfileDto ValidStandardDto() => new BaseExportProfileDto()
        {
            Name = "Stock Level Export",
            Description = "Monthly export",
            ExportType = ExportType.StockLevels,
            CustomSource = null,
            CustomType = null,
            TargetPath = "C:\\Exports\\stock.csv"
        };

        private static BaseExportProfileDto ValidCustomDto() => new BaseExportProfileDto()
        {
            Name = "Custom Export",
            Description = null,
            ExportType = null,
            CustomSource = "custom_my_proc",
            CustomType = CustomType.Procedure,
            TargetPath = "C:\\Exports\\custom.csv"
        };

        // -------------------------------------------------------
        // Name — mirrors: IsRequired(), HasMaxLength(50)
        // -------------------------------------------------------
        [Fact]
        public void Name_Empty_ShouldHaveValidationError()
        {
            var dto = ValidStandardDto();
            dto.Name = "";

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Name_Null_ShouldHaveValidationError()
        {
            var dto = ValidStandardDto();
            dto.Name = null!;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Name_ExactlyMaxLength_ShouldNotHaveValidationError()
        {
            var dto = ValidStandardDto();
            dto.Name = new string('A', 50);

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Name_ExceedsMaxLength_ShouldHaveValidationError()
        {
            var dto = ValidStandardDto();
            dto.Name = new string('A', 51);

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        // -------------------------------------------------------
        // Description — mirrors: HasMaxLength(200)
        // -------------------------------------------------------
        [Fact]
        public void Description_Null_ShouldNotHaveValidationError()
        {
            var dto = ValidStandardDto();
            dto.Description = null;

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Description_ExactlyMaxLength_ShouldNotHaveValidationError()
        {
            var dto = ValidStandardDto();
            dto.Description = new string('A', 200);

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Description_ExceedsMaxLength_ShouldHaveValidationError()
        {
            var dto = ValidStandardDto();
            dto.Description = new string('A', 201);

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        // -------------------------------------------------------
        // CK_ExportProfile_Custom_Source_Or_Export_Type
        // Exactly one of ExportType or CustomSource must be set
        // -------------------------------------------------------
        [Fact]
        public void ExportType_Set_CustomSource_Null_ShouldBeValid()
        {
            var result = _validator.TestValidate(ValidStandardDto());

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void CustomSource_Set_ExportType_Null_ShouldBeValid()
        {
            var result = _validator.TestValidate(ValidCustomDto());

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Both_ExportType_And_CustomSource_Set_ShouldHaveValidationError()
        {
            var dto = ValidStandardDto();
            dto.CustomSource = "custom_my_proc";
            dto.CustomType = CustomType.Procedure;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ExportType);
        }

        [Fact]
        public void Neither_ExportType_Nor_CustomSource_Set_ShouldHaveValidationError()
        {
            var dto = ValidStandardDto();
            dto.ExportType = null;
            dto.CustomSource = null;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ExportType);
        }

        // -------------------------------------------------------
        // CK_ExportProfile_Custom_Source_And_Type
        // CustomSource and CustomType must both be set or both be null
        // -------------------------------------------------------
        [Fact]
        public void CustomSource_Set_CustomType_Null_ShouldHaveValidationError()
        {
            var dto = ValidCustomDto();
            dto.CustomType = null;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.CustomType);
        }

        [Fact]
        public void CustomSource_Null_CustomType_Set_ShouldHaveValidationError()
        {
            var dto = ValidStandardDto();
            dto.CustomType = CustomType.Procedure;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.CustomType);
        }

        [Fact]
        public void Both_CustomSource_And_CustomType_Null_ShouldNotHaveValidationErrorForCustomType()
        {
            var dto = ValidStandardDto();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.CustomType);
        }

        [Fact]
        public void Both_CustomSource_And_CustomType_Set_ShouldNotHaveValidationErrorForCustomType()
        {
            var result = _validator.TestValidate(ValidCustomDto());

            result.ShouldNotHaveValidationErrorFor(x => x.CustomType);
        }

        // -------------------------------------------------------
        // Combined valid scenarios
        // -------------------------------------------------------
        [Fact]
        public void StandardProfile_AllFieldsValid_ShouldHaveNoErrors()
        {
            var result = _validator.TestValidate(ValidStandardDto());

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void CustomProfile_AllFieldsValid_ShouldHaveNoErrors()
        {
            var result = _validator.TestValidate(ValidCustomDto());

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}