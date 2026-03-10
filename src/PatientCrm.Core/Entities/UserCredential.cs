namespace PatientCrm.Core.Entities;

/// <summary>Stored FIDO2 / passkey credential for a user.</summary>
public class UserCredential
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }

    /// <summary>FIDO2 credential ID (base64url-encoded for storage).</summary>
    public required string CredentialId { get; set; }

    /// <summary>CBOR-encoded public key.</summary>
    public required byte[] PublicKey { get; set; }

    public uint SignatureCounter { get; set; }

    /// <summary>Authenticator Attestation GUID.</summary>
    public string? AaGuid { get; set; }

    /// <summary>User-visible label, e.g. "Touch ID on MacBook".</summary>
    public string? DisplayName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUsedAt { get; set; }
}
