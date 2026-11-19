using System.ComponentModel.DataAnnotations;
namespace Veterinaria.Entidades.DTOs;
public sealed class RegistroDto { [Required, EmailAddress, StringLength(150)] public string Correo {get;set;}=""; [Required, MinLength(8)] public string Clave {get;set;}=""; }
public sealed class LoginDto { [Required, EmailAddress] public string Correo {get;set;}=""; [Required] public string Clave {get;set;}=""; }
public sealed record TokenDto(string Token, DateTime ExpiraEn);
public sealed record PerfilDto(string Correo, string Rol);
