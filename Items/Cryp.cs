using Microsoft.AspNetCore.Identity;

namespace Tarefa.Items
{
    public class Cryp
    {
        private readonly IPasswordHasher<object> Hash;

        public Cryp()
        {
            Hash = new PasswordHasher<object>();
        }

        public string Hashar(string Senha)
        {
            return Hash.HashPassword(null, Senha);
        }

        public bool Verify(string Senha, string HashSenha)
        {
            var verificado = Hash.VerifyHashedPassword(null, HashSenha, Senha);
            return verificado == PasswordVerificationResult.Success;
        } 
    }
}
