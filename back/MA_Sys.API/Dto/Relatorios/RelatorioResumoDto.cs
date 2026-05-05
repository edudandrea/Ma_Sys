namespace MA_Sys.API.Dto.Relatorios
{
    public class RelatorioResumoDto
    {
        public string Escopo { get; set; } = "";
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public int TotalCadastrados { get; set; }
        public int TotalAtivos { get; set; }
        public int TotalInativos { get; set; }
        public int TotalEmDia { get; set; }
        public int TotalPendentes { get; set; }
        public int TotalEmAtraso { get; set; }
        public int TotalMensalidadesSistema { get; set; }
        public int TotalMensalidadesSistemaPagas { get; set; }
        public int TotalMensalidadesSistemaPendentes { get; set; }
        public int TotalMensalidadesSistemaAtrasadas { get; set; }
        public decimal ValorMensalidadesSistema { get; set; }
        public decimal ValorMensalidadesSistemaRecebido { get; set; }
        public decimal ValorMensalidadesSistemaPendente { get; set; }
    }
}
