namespace TestDll
{
    public class Func
    {
        public async Task<(bool, float)> AddVerify(int a1, int a2, int validationItem)
        {
            var rs = a1 + a2;
            return await Task.FromResult((rs == validationItem, rs));
        }
    }
}
