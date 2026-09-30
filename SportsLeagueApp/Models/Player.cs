using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SportsLeagueApp.Models
{
    public class Player
    {
        public int Id { get; set; }

        private string? _firstName;
        [Required(ErrorMessage = "Заповніть інформацію про ім'я")]
        public string? FirstName
        {
            get => _firstName;
            set => _firstName = CapitalizeFirst(value);
        }

        private string? _lastName;
        [Required(ErrorMessage = "Заповніть інформацію про прізвище")]
        public string? LastName
        {
            get => _lastName;
            set => _lastName = CapitalizeFirst(value);
        }

        [Required(ErrorMessage = "Заповніть інформацію про дату народження")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        private string? _position;
        [Required(ErrorMessage = "Заповніть інформацію про позицію")]
        public string? Position
        {
            get => _position;
            set => _position = CapitalizeFirst(value);
        }

        [Required(ErrorMessage = "Вкажіть ігровий номер")]
        [Range(0, 99, ErrorMessage = "Ігровий номер має бути від 0 до 99")]
        public int? JerseyNumber { get; set; }

        public List<Team> Teams { get; set; } = new List<Team>();

        private static string? CapitalizeFirst(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;
            s = s.Trim();
            return char.ToUpper(s[0]) + (s.Length > 1 ? s.Substring(1) : string.Empty);
        }
    }
}