namespace MA_Sys.API.Dto.Dashboard
{
    public class FederacaoDashboardDto
    {
        public int TotalFiliados { get; set; }
        public int TotalFiliadosAtivos { get; set; }
        public int TotalFiliadosInativos { get; set; }
        public int TotalCobrancas { get; set; }
        public int TotalCobrancasPagas { get; set; }
        public int TotalCobrancasPendentes { get; set; }
        public int TotalCobrancasVencidas { get; set; }
        public decimal ReceitasMes { get; set; }
        public decimal ValorPendente { get; set; }
        public decimal SaldoCaixa { get; set; }
        public List<string> Meses { get; set; } = [];
        public List<int> FiliadosPorMes { get; set; } = [];
        public List<FiliadoStatusChartDto> CobrancasPorStatus { get; set; } = [];
        public List<FiliadoCobrancaAlertaDto> CobrancasAlerta { get; set; } = [];
        public List<FiliadoCobrancaAlertaDto> UltimasCobrancas { get; set; } = [];
    }
}
