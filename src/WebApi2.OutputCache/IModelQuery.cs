namespace WebAPI.OutputCache
{
    internal interface IModelQuery<in TModel, out TResult>
    {
        TResult Execute(TModel model);
    }
}