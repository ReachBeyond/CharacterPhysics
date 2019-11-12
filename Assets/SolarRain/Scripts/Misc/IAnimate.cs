
public interface IAnimate {
	/// <summary>
	/// Updates some value of an animation that corresponds with the
	/// progress of some other action.
	/// </summary>
	/// <param name="progress">
	/// Closeness to completion.
	/// 0 means just started, and 1 means finished.
	/// </param>
	void UpdateAnimation(float progress);
}
