using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace WebApplication1;

public class TourJwtOptions : TokenValidationParameters
{
    public TourJwtOptions()
    {
        ValidateIssuer = true;
        ValidIssuer = Issuer;
        ValidateAudience = true;
        ValidAudience = Audience;
        IssuerSigningKey = GetSymmetricSecurityKey();
        ValidateIssuerSigningKey = true;
    }

    public const string Issuer = "TourApi";
    public const string Audience = "TourClient"; 
    const string Key = "SuperSecretKey1488";
    
    public static SymmetricSecurityKey GetSymmetricSecurityKey()
    {
        return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Key));
    }
}