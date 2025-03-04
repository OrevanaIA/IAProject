# Guía de Configuración de appsettings.json para JWT

## Introducción

En una aplicación ASP.NET Core, el archivo `appsettings.json` es fundamental para la configuración de la aplicación. Para implementar la autenticación y autorización con JWT, necesitamos configurar correctamente este archivo con las claves y parámetros necesarios.

## Estructura de los archivos de configuración

ASP.NET Core utiliza dos archivos principales de configuración:

1. **appsettings.json**: Contiene la configuración base para todos los entornos.
2. **appsettings.Development.json**: Contiene configuraciones específicas para el entorno de desarrollo, que sobrescriben las configuraciones base.

## Configuración para JWT

### appsettings.json

```json
{
  "JwtSettings": {
    "Key": "YourSuperSecretKeyHereMakeItLongAndComplex123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ",
    "Issuer": "AIModulo03",
    "Audience": "AIModulo03Users",
    "DurationInMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

### appsettings.Development.json

```json
{
  "JwtSettings": {
    "Key": "DevEnvironmentSecretKey123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ",
    "Issuer": "AIModulo03.Development",
    "Audience": "AIModulo03Users.Development",
    "DurationInMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
```

## Explicación de los parámetros JWT

- **Key**: Es la clave secreta utilizada para firmar y verificar los tokens JWT. Debe ser una cadena larga y compleja para garantizar la seguridad.
  
- **Issuer**: Identifica quién emitió el token. Generalmente es el nombre de tu aplicación o servicio.
  
- **Audience**: Identifica para quién está destinado el token. Puede ser el nombre de tu aplicación cliente o un grupo de usuarios.
  
- **DurationInMinutes**: Define cuánto tiempo será válido el token antes de expirar.

## Cómo se utilizan estos valores en el código

En el servicio de autenticación (`AuthService.cs`), estos valores se utilizan para generar y validar tokens JWT:

```csharp
public string GenerateJwtToken(User user)
{
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role)
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
        _configuration.GetSection("JwtSettings:Key").Value));

    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.UtcNow.AddMinutes(
            Convert.ToDouble(_configuration.GetSection("JwtSettings:DurationInMinutes").Value)),
        SigningCredentials = creds,
        Issuer = _configuration.GetSection("JwtSettings:Issuer").Value,
        Audience = _configuration.GetSection("JwtSettings:Audience").Value
    };

    var tokenHandler = new JwtSecurityTokenHandler();
    var token = tokenHandler.CreateToken(tokenDescriptor);

    return tokenHandler.WriteToken(token);
}
```

En la configuración de la aplicación (`Program.cs`), estos valores se utilizan para configurar la validación de tokens:

```csharp
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            builder.Configuration.GetSection("JwtSettings:Key").Value)),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration.GetSection("JwtSettings:Issuer").Value,
        ValidateAudience = true,
        ValidAudience = builder.Configuration.GetSection("JwtSettings:Audience").Value,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});
```

## Mejores prácticas de seguridad

1. **No almacenar claves secretas en el control de versiones**: Nunca incluya claves secretas reales en los archivos de configuración que se suben a repositorios de código. Use variables de entorno o un gestor de secretos como Azure Key Vault o AWS Secrets Manager.

2. **Usar claves diferentes para cada entorno**: Utilice claves diferentes para desarrollo, pruebas y producción.

3. **Rotar claves periódicamente**: Cambie las claves de firma JWT regularmente para minimizar el riesgo de compromiso.

4. **Usar claves largas y complejas**: Las claves deben tener al menos 256 bits (32 caracteres) de entropía para ser seguras contra ataques de fuerza bruta.

5. **Configurar la expiración adecuada**: Los tokens no deben tener una vida útil demasiado larga. Para la mayoría de las aplicaciones web, 1 hora es un buen equilibrio.

## Configuración en entornos de producción

En entornos de producción, es recomendable no almacenar la clave secreta en el archivo `appsettings.json`, sino utilizar variables de entorno o un gestor de secretos. Puede modificar su código para leer la clave de una variable de entorno:

```csharp
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
    Environment.GetEnvironmentVariable("JWT_KEY") ?? 
    _configuration.GetSection("JwtSettings:Key").Value));
```

Y luego configurar la variable de entorno en su servidor de producción:

```bash
export JWT_KEY="YourProductionSecretKey"
```

O en Windows:

```cmd
set JWT_KEY=YourProductionSecretKey
```

## Conclusión

La correcta configuración de los archivos `appsettings.json` es crucial para implementar un sistema de autenticación JWT seguro y funcional. Asegúrese de seguir las mejores prácticas de seguridad, especialmente en entornos de producción, para proteger las claves secretas y garantizar la integridad de su sistema de autenticación.
