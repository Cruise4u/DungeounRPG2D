/// <summary>
/// Implement on any component that needs to resolve dependencies when a pooled
/// instance is first created, regardless of whether the instance ends up disabled
/// (e.g. parented under an inactive pool container) before Awake/OnEnable would fire.
/// </summary>
public interface IPoolSetup
{
    void OnPoolSetup();
}
