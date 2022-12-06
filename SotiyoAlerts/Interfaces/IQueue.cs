namespace SotiyoAlerts.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IQueue<in T> where T : class
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        void Enqueue(T item);
    }
}