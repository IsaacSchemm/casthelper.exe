namespace System.Reactive.Linq { }

namespace System.Reactive.Disposables {
	public static class Disposable {
		private sealed class EmptyDisposable : IDisposable {
			private EmptyDisposable() { }

			public void Dispose() { }

			public static readonly EmptyDisposable Singleton = new EmptyDisposable();
		}

		public static IDisposable Empty => EmptyDisposable.Singleton;
	}
}
