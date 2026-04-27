using Application.Entities.Core.ScheduledJobs.DTOs;
using Application.Entities.Core.ScheduledJobs.Features.Commands;
using Application.Entities.Core.ScheduledJobs.Validators;
using Domain.Enums;
using FluentValidation.TestHelper;

namespace Test.Unit.Validators.Scheduler
{
    public class ScheduledJobValidatorTest
    {
        // -------------------------------------------------------
        // Helpers
        // -------------------------------------------------------
        private static BaseScheduledJobDto ValidImportDto() => new BaseScheduledJobDto()
        {
            Name = "Import Products",
            Description = "Monthly import",
            JobType = JobType.Import,
            ImportDataType = ImportDataType.Products,
            Source = "C:\\Data\\products.csv",
            CalculationType = null,
            Target = null
        };

        private static BaseScheduledJobDto ValidCalculationDto() => new BaseScheduledJobDto()
        {
            Name = "Calculate Stock Levels",
            Description = null,
            JobType = JobType.StandardCalculation,
            CalculationType = StandardCalculationType.StockLevels,
            ImportDataType = null,
            Source = null,
            Target = null
        };

        private static BaseScheduledJobDto ValidCustomDto() => new BaseScheduledJobDto()
        {
            Name = "Custom Calculation",
            Description = null,
            JobType = JobType.CustomCalculation,
            Target = "custom_my_proc",
            ImportDataType = null,
            Source = null,
            CalculationType = null
        };

        // -------------------------------------------------------
        // BaseScheduledJobDtoValidator — Name
        // -------------------------------------------------------
        [Fact]
        public void Name_Empty_ShouldHaveValidationError()
        {
            var validator = new BaseScheduledJobDtoValidator();
            var dto = ValidImportDto();
            dto.Name = "";

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Name_Null_ShouldHaveValidationError()
        {
            var validator = new BaseScheduledJobDtoValidator();
            var dto = ValidImportDto();
            dto.Name = null!;

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Name_ExactlyMaxLength_ShouldNotHaveValidationError()
        {
            var validator = new BaseScheduledJobDtoValidator();
            var dto = ValidImportDto();
            dto.Name = new string('A', 50);

            var result = validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Name_ExceedsMaxLength_ShouldHaveValidationError()
        {
            var validator = new BaseScheduledJobDtoValidator();
            var dto = ValidImportDto();
            dto.Name = new string('A', 51);

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        // -------------------------------------------------------
        // BaseScheduledJobDtoValidator — Description
        // -------------------------------------------------------
        [Fact]
        public void Description_Null_ShouldNotHaveValidationError()
        {
            var validator = new BaseScheduledJobDtoValidator();
            var dto = ValidImportDto();
            dto.Description = null;

            var result = validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Description_ExactlyMaxLength_ShouldNotHaveValidationError()
        {
            var validator = new BaseScheduledJobDtoValidator();
            var dto = ValidImportDto();
            dto.Description = new string('A', 200);

            var result = validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Description_ExceedsMaxLength_ShouldHaveValidationError()
        {
            var validator = new BaseScheduledJobDtoValidator();
            var dto = ValidImportDto();
            dto.Description = new string('A', 201);

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        // -------------------------------------------------------
        // BaseScheduledJobDtoValidator — JobType
        // -------------------------------------------------------
        [Fact]
        public void JobType_InvalidValue_ShouldHaveValidationError()
        {
            var validator = new BaseScheduledJobDtoValidator();
            var dto = ValidImportDto();
            dto.JobType = (JobType) 99;

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.JobType);
        }

        [Theory]
        [InlineData(JobType.Import)]
        [InlineData(JobType.StandardCalculation)]
        [InlineData(JobType.CustomCalculation)]
        public void JobType_ValidValue_ShouldNotHaveValidationError(JobType jobType)
        {
            var validator = new BaseScheduledJobDtoValidator();

            var dto = jobType switch
            {
                JobType.Import => ValidImportDto(),
                JobType.StandardCalculation => ValidCalculationDto(),
                JobType.CustomCalculation => ValidCustomDto(),
                _ => ValidImportDto()
            };

            var result = validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.JobType);
        }

        // -------------------------------------------------------
        // BaseScheduledJobDtoValidator — Import conditional rules
        // -------------------------------------------------------
        [Theory]
        [InlineData(null)]
        [InlineData((ImportDataType)99)]
        public void Import_ImportDataType_Invalid_ShouldHaveValidationError(ImportDataType? dataType)
        {
            var validator = new BaseScheduledJobDtoValidator();
            var dto = ValidImportDto();
            dto.ImportDataType = dataType;

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ImportDataType);
        }

        [Fact]
        public void Import_Source_Empty_ShouldHaveValidationError()
        {
            var validator = new BaseScheduledJobDtoValidator();
            var dto = ValidImportDto();
            dto.Source = "";

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Source);
        }

        [Fact]
        public void Import_Source_Null_ShouldHaveValidationError()
        {
            var validator = new BaseScheduledJobDtoValidator();
            var dto = ValidImportDto();
            dto.Source = null;

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Source);
        }

        [Theory]
        [InlineData(ImportDataType.Products)]
        [InlineData(ImportDataType.Transactions)]
        [InlineData(ImportDataType.ConfirmedOrders)]
        [InlineData(ImportDataType.PurchaseOrders)]
        [InlineData(ImportDataType.UserDefinedFields)]
        public void Import_AllValidDataTypes_ShouldNotHaveValidationError(ImportDataType dataType)
        {
            var validator = new BaseScheduledJobDtoValidator();
            var dto = ValidImportDto();
            dto.ImportDataType = dataType;

            var result = validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.ImportDataType);
        }

        [Fact]
        public void Import_AllFieldsValid_ShouldHaveNoErrors()
        {
            var validator = new BaseScheduledJobDtoValidator();

            var result = validator.TestValidate(ValidImportDto());

            result.ShouldNotHaveAnyValidationErrors();
        }

        // --------------------------------------------------------------------
        // BaseScheduledJobDtoValidator — StandardCalculation conditional rules
        // --------------------------------------------------------------------
        [Theory]
        [InlineData(null)]
        [InlineData((StandardCalculationType) 99)]
        public void StandardCalculation_CalculationType_Null_ShouldHaveValidationError(StandardCalculationType? calculationType)
        {
            var validator = new BaseScheduledJobDtoValidator();
            var dto = ValidCalculationDto();
            dto.CalculationType = calculationType;

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.CalculationType);
        }

        [Theory]
        [InlineData(StandardCalculationType.StockLevels)]
        [InlineData(StandardCalculationType.ConfirmedOrderAggregates)]
        [InlineData(StandardCalculationType.Forecasts)]
        [InlineData(StandardCalculationType.SalesAggregates)]
        [InlineData(StandardCalculationType.PurchaseForecasts)]
        [InlineData(StandardCalculationType.Seasonality)]
        [InlineData(StandardCalculationType.ProductKpis)]
        public void StandardCalculation_AllValidTypes_ShouldNotHaveValidationError(
            StandardCalculationType calculationType)
        {
            var validator = new BaseScheduledJobDtoValidator();
            var dto = ValidCalculationDto();
            dto.CalculationType = calculationType;

            var result = validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.CalculationType);
        }

        [Fact]
        public void StandardCalculation_AllFieldsValid_ShouldHaveNoErrors()
        {
            var validator = new BaseScheduledJobDtoValidator();

            var result = validator.TestValidate(ValidCalculationDto());

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ------------------------------------------------------------------
        // BaseScheduledJobDtoValidator — CustomCalculation conditional rules
        // ------------------------------------------------------------------
        [Fact]
        public void CustomCalculation_Target_Empty_ShouldHaveValidationError()
        {
            var validator = new BaseScheduledJobDtoValidator();
            var dto = ValidCustomDto();
            dto.Target = "";

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Target);
        }

        [Fact]
        public void CustomCalculation_Target_Null_ShouldHaveValidationError()
        {
            var validator = new BaseScheduledJobDtoValidator();
            var dto = ValidCustomDto();
            dto.Target = null;

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Target);
        }

        [Fact]
        public void CustomCalculation_AllFieldsValid_ShouldHaveNoErrors()
        {
            var validator = new BaseScheduledJobDtoValidator();

            var result = validator.TestValidate(ValidCustomDto());

            result.ShouldNotHaveAnyValidationErrors();
        }

        // -------------------------------------------------------
        // Conditional rules only fire for the correct JobType
        // -------------------------------------------------------
        [Fact]
        public void Import_CalculationType_Null_ShouldNotHaveValidationError()
        {
            // CalculationType is only required for StandardCalculation, not Import/Custom
            var validator = new BaseScheduledJobDtoValidator();
            var dto = ValidImportDto();
            dto.CalculationType = null;

            var result = validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.CalculationType);
        }

        [Fact]
        public void StandardCalculation_ImportDataType_Null_ShouldNotHaveValidationError()
        {
            // ImportDataType is only required for Import, not StandardCalculation/Custom
            var validator = new BaseScheduledJobDtoValidator();
            var dto = ValidCalculationDto();
            dto.ImportDataType = null;

            var result = validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.ImportDataType);
        }

        [Fact]
        public void StandardCalculation_Target_Null_ShouldNotHaveValidationError()
        {
            // Target is only required for CustomCalculation, not StandardCalculation/Import
            var validator = new BaseScheduledJobDtoValidator();
            var dto = ValidCalculationDto();
            dto.Target = null;

            var result = validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Target);
        }

        // -------------------------------------------------------
        // CreateScheduledJobValidator
        // -------------------------------------------------------
        [Fact]
        public void Create_JobTaskKey_Empty_ShouldHaveValidationError()
        {
            var validator = new CreateScheduledJobValidator();
            var command = new CreateScheduledJob.Command
            {
                CreateScheduledJobDto = new CreateScheduledJobDto
                {
                    Name = "Import Products",
                    JobType = JobType.Import,
                    ImportDataType = ImportDataType.Products,
                    Source = "C:\\Data\\products.csv",
                    JobTaskKey = ""
                }
            };

            var result = validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(
                x => x.CreateScheduledJobDto.JobTaskKey);
        }

        [Fact]
        public void Create_JobTaskKey_ExceedsMaxLength_ShouldHaveValidationError()
        {
            var validator = new CreateScheduledJobValidator();
            var command = new CreateScheduledJob.Command
            {
                CreateScheduledJobDto = new CreateScheduledJobDto
                {
                    Name = "Import Products",
                    JobType = JobType.Import,
                    ImportDataType = ImportDataType.Products,
                    Source = "C:\\Data\\products.csv",
                    JobTaskKey = new string('A', 51)
                }
            };

            var result = validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(
                x => x.CreateScheduledJobDto.JobTaskKey);
        }

        [Fact]
        public void Create_AllFieldsValid_ShouldHaveNoErrors()
        {
            var validator = new CreateScheduledJobValidator();
            var command = new CreateScheduledJob.Command
            {
                CreateScheduledJobDto = new CreateScheduledJobDto
                {
                    Name = "Import Products",
                    JobType = JobType.Import,
                    ImportDataType = ImportDataType.Products,
                    Source = "C:\\Data\\products.csv",
                    JobTaskKey = "JT-1"
                }
            };

            var result = validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        // -------------------------------------------------------
        // EditScheduledJobValidator
        // -------------------------------------------------------
        [Fact]
        public void Edit_IsArchived_ValidValue_ShouldNotHaveValidationError()
        {
            var validator = new EditScheduledJobValidator();
            var command = new EditScheduledJob.Command
            {
                EditScheduledJobDto = new EditScheduledJobDto
                {
                    Name = "Import Products",
                    JobType = JobType.Import,
                    ImportDataType = ImportDataType.Products,
                    Source = "C:\\Data\\products.csv",
                    IsArchived = false,
                    JobTaskKey = "JT-1"
                }
            };

            var result = validator.TestValidate(command);

            result.ShouldNotHaveValidationErrorFor(
                x => x.EditScheduledJobDto.IsArchived);
        }

        [Fact]
        public void Edit_AllFieldsValid_ShouldHaveNoErrors()
        {
            var validator = new EditScheduledJobValidator();
            var command = new EditScheduledJob.Command
            {
                EditScheduledJobDto = new EditScheduledJobDto
                {
                    Name = "Import Products",
                    JobType = JobType.Import,
                    ImportDataType = ImportDataType.Products,
                    Source = "C:\\Data\\products.csv",
                    IsArchived = false,
                    JobTaskKey = "JT-1"
                }
            };

            var result = validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        // -------------------------------------------------------
        // ReorderScheduledJobsValidator
        // -------------------------------------------------------
        [Fact]
        public void Reorder_EmptyList_ShouldHaveValidationError()
        {
            var validator = new ReorderScheduledJobsValidator();
            var command = new ReorderScheduledJobs.Command
            {
                Dto = new ReorderScheduledJobsDto
                {
                    OrderedScheduledJobIds = new List<Guid>()
                }
            };

            var result = validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Dto.OrderedScheduledJobIds);
        }

        [Fact]
        public void Reorder_SingleId_ShouldNotHaveValidationError()
        {
            var validator = new ReorderScheduledJobsValidator();
            var command = new ReorderScheduledJobs.Command
            {
                Dto = new ReorderScheduledJobsDto
                {
                    OrderedScheduledJobIds = new List<Guid> { Guid.NewGuid() }
                }
            };

            var result = validator.TestValidate(command);

            result.ShouldNotHaveValidationErrorFor(x => x.Dto.OrderedScheduledJobIds);
        }

        [Fact]
        public void Reorder_MultipleIds_ShouldNotHaveValidationError()
        {
            var validator = new ReorderScheduledJobsValidator();
            var command = new ReorderScheduledJobs.Command
            {
                Dto = new ReorderScheduledJobsDto
                {
                    OrderedScheduledJobIds = new List<Guid>
                    {
                        Guid.NewGuid(),
                        Guid.NewGuid(),
                        Guid.NewGuid()
                    }
                }
            };

            var result = validator.TestValidate(command);

            result.ShouldNotHaveValidationErrorFor(x => x.Dto.OrderedScheduledJobIds);
        }
    }
}