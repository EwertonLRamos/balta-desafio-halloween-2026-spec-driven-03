namespace Autenticador.IntegrationTests.Fixtures;

public static class TestAccountsFixture
{
    public static IReadOnlyList<TestAccountData> Accounts { get; } =
    [
        new("admin@teste", true),
        new("operador@teste", true),
        new("inativo@teste", false)
    ];
}
