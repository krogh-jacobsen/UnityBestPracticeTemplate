# Sample: UI Toolkit MVP

Demonstrates the Model-View-Presenter (MVP) pattern with UI Toolkit. The View owns the UIDocument; the Presenter owns the logic. They communicate via events and method calls — never direct field access.

## How it works

- `SampleView` (MonoBehaviour) queries UXML elements and exposes `event Action` for user interactions
- `SamplePresenter` (plain C#) subscribes to view events, updates model state, calls view methods to update the display
- The UXML defines structure; the USS defines appearance using BEM naming and CSS variables

## Setup

1. Create a `UIDocument` GameObject in your scene
2. Assign `SampleView.uxml` as the source asset
3. Attach `SampleView.cs` to the same GameObject
4. Press Play — clicking **Increment** increments the counter via the presenter

## Key takeaways

- View has no business logic — only queries, exposes events, and updates display
- Presenter has no Unity dependencies — easier to unit test
- Subscribe view events in `OnEnable`, unsubscribe and dispose presenter in `OnDisable`
- Use BEM naming in UXML/USS: `block__element--modifier`
- CSS variables (`--color-primary`) for maintainable theming
