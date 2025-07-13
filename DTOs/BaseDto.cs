using System;

namespace PengerAPI.DTOs
{
    public abstract class BaseDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public abstract class BaseCreateDto
    {
        // Base class for create DTOs - no Id or timestamps
    }

    public abstract class BaseUpdateDto
    {
        public int Id { get; set; }
        // UpdatedAt will be set automatically in the service layer
    }
}
