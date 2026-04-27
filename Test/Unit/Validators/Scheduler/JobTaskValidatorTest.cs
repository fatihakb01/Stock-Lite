using Application.Entities.Core.JobTasks.DTOs;
using Application.Entities.Core.JobTasks.Features.Commands;
using Application.Entities.Core.JobTasks.Validators;
using FluentValidation.TestHelper;

namespace Test.Unit.Validators.Scheduler
{
    public class JobTaskValidatorTest
    {
        // -------------------------------------------------------
        // Helpers
        // -------------------------------------------------------
        private static BaseJobTaskDto ValidDto() => new BaseJobTaskDto()
        {
            Name = "Import Job Task",
            Description = "Daily import",
            JobTaskKey = "JT-1"
        };

        // -------------------------------------------------------
        // BaseJobTaskDtoValidator — Name
        // -------------------------------------------------------
        [Fact]
        public void Name_Empty_ShouldHaveValidationError()
        {
            var validator = new BaseJobTaskDtoValidator();
            var dto = ValidDto();
            dto.Name = "";

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Name_Null_ShouldHaveValidationError()
        {
            var validator = new BaseJobTaskDtoValidator();
            var dto = ValidDto();
            dto.Name = null!;

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Name_ExactlyMaxLength_ShouldNotHaveValidationError()
        {
            var validator = new BaseJobTaskDtoValidator();
            var dto = ValidDto();
            dto.Name = new string('A', 50);

            var result = validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Name_ExceedsMaxLength_ShouldHaveValidationError()
        {
            var validator = new BaseJobTaskDtoValidator();
            var dto = ValidDto();
            dto.Name = new string('A', 51);

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        // -------------------------------------------------------
        // BaseJobTaskDtoValidator — Description
        // -------------------------------------------------------
        [Fact]
        public void Description_Null_ShouldNotHaveValidationError()
        {
            var validator = new BaseJobTaskDtoValidator();
            var dto = ValidDto();
            dto.Description = null;

            var result = validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Description_ExactlyMaxLength_ShouldNotHaveValidationError()
        {
            var validator = new BaseJobTaskDtoValidator();
            var dto = ValidDto();
            dto.Description = new string('A', 200);

            var result = validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Description_ExceedsMaxLength_ShouldHaveValidationError()
        {
            var validator = new BaseJobTaskDtoValidator();
            var dto = ValidDto();
            dto.Description = new string('A', 201);

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        // -------------------------------------------------------
        // BaseJobTaskDtoValidator — JobTaskKey
        // -------------------------------------------------------
        [Fact]
        public void JobTaskKey_Empty_ShouldHaveValidationError()
        {
            var validator = new BaseJobTaskDtoValidator();
            var dto = ValidDto();
            dto.JobTaskKey = "";

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.JobTaskKey);
        }

        [Fact]
        public void JobTaskKey_Null_ShouldHaveValidationError()
        {
            var validator = new BaseJobTaskDtoValidator();
            var dto = ValidDto();
            dto.JobTaskKey = null!;

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.JobTaskKey);
        }

        [Fact]
        public void JobTaskKey_ExactlyMaxLength_ShouldNotHaveValidationError()
        {
            var validator = new BaseJobTaskDtoValidator();
            var dto = ValidDto();
            dto.JobTaskKey = new string('A', 50);

            var result = validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.JobTaskKey);
        }

        [Fact]
        public void JobTaskKey_ExceedsMaxLength_ShouldHaveValidationError()
        {
            var validator = new BaseJobTaskDtoValidator();
            var dto = ValidDto();
            dto.JobTaskKey = new string('A', 51);

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.JobTaskKey);
        }

        // -------------------------------------------------------
        // BaseJobTaskDtoValidator — all fields valid
        // -------------------------------------------------------
        [Fact]
        public void AllFieldsValid_ShouldHaveNoErrors()
        {
            var validator = new BaseJobTaskDtoValidator();

            var result = validator.TestValidate(ValidDto());

            result.ShouldNotHaveAnyValidationErrors();
        }

        // -------------------------------------------------------
        // CreateJobTaskValidator
        // -------------------------------------------------------
        [Fact]
        public void Create_AllFieldsValid_ShouldHaveNoErrors()
        {
            var validator = new CreateJobTaskValidator();
            var command = new CreateJobTask.Command
            {
                CreateJobTaskDto = new CreateJobTaskDto
                {
                    Name = "Import Job Task",
                    JobTaskKey = "JT-1"
                }
            };

            var result = validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Create_Name_Empty_ShouldHaveValidationError()
        {
            var validator = new CreateJobTaskValidator();
            var command = new CreateJobTask.Command
            {
                CreateJobTaskDto = new CreateJobTaskDto
                {
                    Name = "",
                    JobTaskKey = "JT-1"
                }
            };

            var result = validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.CreateJobTaskDto.Name);
        }

        [Fact]
        public void Create_JobTaskKey_Empty_ShouldHaveValidationError()
        {
            var validator = new CreateJobTaskValidator();
            var command = new CreateJobTask.Command
            {
                CreateJobTaskDto = new CreateJobTaskDto
                {
                    Name = "Import Job Task",
                    JobTaskKey = ""
                }
            };

            var result = validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.CreateJobTaskDto.JobTaskKey);
        }

        // -------------------------------------------------------
        // EditJobTaskValidator
        // -------------------------------------------------------
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Edit_IsArchived_ValidValue_ShouldNotHaveValidationError(bool isArchived)
        {
            var validator = new EditJobTaskValidator();
            var command = new EditJobTask.Command
            {
                EditJobTaskDto = new EditJobTaskDto
                {
                    Name = "Import Job Task",
                    JobTaskKey = "JT-1",
                    IsArchived = isArchived
                }
            };

            var result = validator.TestValidate(command);

            result.ShouldNotHaveValidationErrorFor(x => x.EditJobTaskDto.IsArchived);
        }

        [Fact]
        public void Edit_AllFieldsValid_ShouldHaveNoErrors()
        {
            var validator = new EditJobTaskValidator();
            var command = new EditJobTask.Command
            {
                EditJobTaskDto = new EditJobTaskDto
                {
                    Name = "Import Job Task",
                    JobTaskKey = "JT-1",
                    IsArchived = false
                }
            };

            var result = validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Edit_Name_Empty_ShouldHaveValidationError()
        {
            var validator = new EditJobTaskValidator();
            var command = new EditJobTask.Command
            {
                EditJobTaskDto = new EditJobTaskDto
                {
                    Name = "",
                    JobTaskKey = "JT-1",
                    IsArchived = false
                }
            };

            var result = validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.EditJobTaskDto.Name);
        }

        [Fact]
        public void Edit_JobTaskKey_Empty_ShouldHaveValidationError()
        {
            var validator = new EditJobTaskValidator();
            var command = new EditJobTask.Command
            {
                EditJobTaskDto = new EditJobTaskDto
                {
                    Name = "Import Job Task",
                    JobTaskKey = "",
                    IsArchived = false
                }
            };

            var result = validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.EditJobTaskDto.JobTaskKey);
        }
    }
}