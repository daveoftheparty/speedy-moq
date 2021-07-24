namespace HowCtorInjectWorks
{
    public interface IMakeItSo {}

    public interface IToldYouSo {}

    public class Hi
    {
        private readonly IToldYouSo _toldYouSo;
        private readonly IMakeItSo _makeItSo;

		public Hi(IMakeItSo makeItSo, IToldYouSo toldYouSo)
		{
			_makeItSo = makeItSo;
			_toldYouSo = toldYouSo;
		}
	}
}