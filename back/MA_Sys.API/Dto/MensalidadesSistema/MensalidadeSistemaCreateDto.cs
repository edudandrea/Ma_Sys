namespace MA_Sys.API.Dto.MensalidadesSistema
{
    public class MensalidadeSistemaCreateDto
    {
        public decimal Valor { get; set; }
        public int PrazoPagamentoDias { get; set; }
        public int MesesUso { get; set; }
        public string? Descricao { get; set; }
    }
}
