if ( !isObject(TemplateBehavior) )
{
	%template = new BehaviorTemplate(TemplateBehavior);
	
	%template.friendlyName = "Template Behavior";
	%template.behaviorType = "Template";
	%template.description = "A template for making behaviors.";
	
	%template.addBehaviorField(templateField, "A template field.", float, 1.0);
}

function TemplateBehavior::onBehaviorAdd(%this)
{
	// Insert instantiation behavior here.
}

function TemplateBehavior::onBehaviorRemove(%this)
{
	// Insert deletion behavior here.
}