using Il2Cpp;

namespace Moment
{
    public static class Check
	{
		public static bool InGame => !GameManager.IsMainMenuActive() && !GameManager.IsBootSceneActive() && !GameManager.IsEmptySceneActive();

	}
}
