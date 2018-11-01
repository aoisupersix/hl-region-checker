
namespace HLRegionChecker.Interfaces
{
    /// <summary>
    /// ジオフェンスの登録を行うDependencyServiceのインタフェース
    /// Android限定で動作させます。
    /// </summary>
    public interface IRegisterGeofences
    {
        /// <summary>
        /// ジオフェンスの登録を行います。
        /// </summary>
        void Register();
    }
}
