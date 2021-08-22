# Uno.UI - Performance

This article lists a number of performance tips to optimize your Uno Platform application. 

Here's what to look for:
- Make sure to always have the simplest visual tree. There's nothing faster than something you don't draw.
- Reduce panels in panels depth. Use Grids and relative panels where possible.
- Force the size of images anywhere possible
- When binding the `Visibility` property, make sure to always set TargetNullValue and FallbackValue to collapsed :
	`Visibility="{Binding [IsAvailable], Converter={StaticResource boolToVisibility}, FallbackValue=Collapsed, TargetNullValue=Collapsed}"`
- Collapsed elements are not considered when measuring and arranging the visual tree, which makes them almost costless.
- When binding or animating (via visual state setters) the visibility property, make sure to enable lazy loading:
	`x:Load="False"`.
- Use `x:Load={x:Bind MyVisibility}` where appropriate as toggling to from true false effectively removes a part of the visual tree from memory. Note that setting back to true re-creates the visual tree.
- ListView and GridView
	- Don't use template selectors inside the ItemTemplate, prefer using the ItemTemplateSelector on ListView/GridView.
	- The default [ListViewItem and GridViewItem styles](https://github.com/unoplatform/uno/blob/74b7d5d0e953fcdd94223f32f51665af7ce15c60/src/Uno.UI/UI/Xaml/Style/Generic/Generic.xaml#L951) are very feature rich, yet that makes them quite slow. For instance, if you know that you're not likely to use selection features for a specific ListView, create a simpler ListViewItem style that some visual states, or the elements that are only used for selection.
- Updating items in `ItemsControl` can be quite expensive, using `ItemsRepeater` is generally faster at rendering similar content.
- Animations
	- Prefer `Opacity` animations to `Visibility` animations (this avoids some measuring performance issues).
		- Unless the visual tree of the element is very big, where in this case `Visibility` is better suited.
	- Prefer Storyboard setters to `ObjectAnimationUsingKeyFrames` if there is only one key frame.
	- Prefer changing the properties of a visual element instead of switching opacity or visibility of an element.
        - Manually created `Storyboard` instances do not stop automatically. Make sure that if you invoke `Storyboard.Begin()`, invoke `Storyboard.Stop()` when the animated content is unloaded, otherwise resources may be spent animating invisible content.
		- `ProgressRing` and `ProgressBar` controls indeterminate mode generally consume rendering time. Make sure to set those to determinate modes when not visible.
		- Troubleshooting of animations can be done by enabling the following logger:
			```csharp
			builder.AddFilter("Windows.UI.Xaml.Media.Animation", LogLevel.Debug);
			```
			The logger will provide all the changes done to animated properties, with element names.
		
- Image Assets
	- Try using an image that is appropriate for the DPI and screen size.
    - Whenever possible, specify and explicit Width and Height on `Image`.
	- The pixel size of an image will impact the loading time of the image. If the image is intentionally blurry, prefer reducing the physical size of the image over 
	  the compressed disk size of the image.
- Paths
	- Prefer reusing paths, duplication costs Main and GPU memory.
	- Prefer using custom fonts over paths where possible. Fonts are rendered extremely fast and have a very low initialization time.
- Bindings
	- Prefer bindings with short paths.
	- To shorten paths, use the `DataContext` property on containers, such as `StackPanel` or `Grid`.
	- As of Uno 3.9, adding a control to loaded `Panel` or `ContentControl` does propagate the parent's DataContext immediately. If the new control has its `DataContext` immediately overriden to something else, ensure to set the DataContext before adding the control to its parent.T his will avoid having bindings be refreshed twice needlessly.
	- Add the `Windows.UI.Xaml.BindableAttribute` or `System.ComponentModel.BindableAttribute` on non-DependencyObject classes.
		- When data binding to classes not inheriting from DependencyObject, in Debug configuration only, the following message may appear:
			```
			The Bindable attribute is missing and the type [XXXX] is not known by the MetadataProvider. 
			Reflection was used instead of the binding engine and generated static metadata. Add the Bindable 	attribute to prevent this message and performance issues.
			```
			This message indicates that the binding engine will fall back on reflection based code, which is generally slow. To compensate for this, Uno use the `BindableTypeProvidersSourceGenerator`, which generates static non-generic code to avoid reflection operations during binding operations.
			This attribute is inherited and is generally used on ViewModel based classes.
- [`x:Phase`](https://docs.microsoft.com/en-us/windows/uwp/xaml-platform/x-phase-attribute)
	- For `ListView` instances with large templates, consider the use of x:Phase to reduce the number of bindings processed during item materialization.
	- It is only supported for items inside `ListViewItem` templates, it will be ignored for others.
	- It is also supported as `xamarin:Phase` on controls that do not have bindings. This feature is not supported by UWP.
	- It is only supported for elements under the `DataTemplate` of a `ListViewItem`. The 
	attribute is ignored for templates of `ContentControl` instances, or any other control.
    - When binding to Brushes with a solid color, prefer binding to the `Color` property like this if the brush type does not change:
    ```xml
    <TextBlock Text="My Text">
        <TextBlock.Foreground>
            <SolidColorBrush Color="{x:Bind Color, Mode=OneWay, FallbackValue=Red}" />
        </TextBlock.Foreground>
    </TextBlock>
    ```

## Advanced performance Tracing

### FrameworkTemplatePool
The framework template pool manages the pooling of ControlTemplates and DataTemplates, and in most cases, the recycling of controls should be high.

- `CreateTemplate` is raised when a new instance of a template is created. This is an expensive operation that should be performed as rarely as possible.
- `RecycleTemplate` is raised when an active instance of a template is placed into the pool. This should happen often.
- `ReuseTemplate` is raised when a pooled template is provided to a control asking for a specific data template.
- `ReleaseTemplate` is raised when a pooled template instance has not been used for a while.

If the `ReuseTemplate` occurrences is low, this usually means that there is a memory leak to investigate.
