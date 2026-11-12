namespace GoldenGym.Domain.Exceptions;

/// <summary>Excepción base del dominio. El middleware de la API la traduce a un código HTTP.</summary>
public abstract class DomainException : Exception
{
    protected DomainException(string mensaje) : base(mensaje) { }
}

/// <summary>404 - El recurso no existe.</summary>
public class NotFoundException : DomainException
{
    public NotFoundException(string mensaje) : base(mensaje) { }
}

/// <summary>409 - Conflicto con datos existentes (ej. correo duplicado).</summary>
public class ConflictException : DomainException
{
    public ConflictException(string mensaje) : base(mensaje) { }
}

/// <summary>422 - Se viola una regla de negocio (ej. membresía vencida).</summary>
public class BusinessRuleException : DomainException
{
    public BusinessRuleException(string mensaje) : base(mensaje) { }
}

/// <summary>403 - El usuario autenticado no tiene permiso sobre ese recurso.</summary>
public class ForbiddenException : DomainException
{
    public ForbiddenException(string mensaje) : base(mensaje) { }
}

/// <summary>401 - Credenciales inválidas o cuenta inactiva.</summary>
public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string mensaje) : base(mensaje) { }
}
