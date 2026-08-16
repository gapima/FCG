using FIAP.CloudGames.Application.Identity.Usuarios;

namespace FIAP.CloudGames.UnitTests.Identity.Usuarios;

public sealed class TestesPoliticaSenha
{
    [Fact]
    public void Validar_ComSenhaForte_NaoRetornaErros()
    {
        var erros = PoliticaSenha.Validar("Senha@123");

        Assert.Empty(erros);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Curta@1")]
    [InlineData("SEMNUMERO@")]
    [InlineData("1234567@")]
    [InlineData("SemEspecial123")]
    public void Validar_QuandoUmaRegraNaoEhAtendida_RetornaErros(string senha)
    {
        var erros = PoliticaSenha.Validar(senha);

        Assert.NotEmpty(erros);
    }
}
