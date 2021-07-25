namespace Features.Model
{
	#warning deprecate this for the Model.Lsp records instead...
	public class FileText
	{
		public string FileName { get; set; }
		public string Text { get; set; }
		public int? Version { get; set; }
	}
}