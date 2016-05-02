$.validator.addMethod("genericcompare", function (value, element, params) {
	var propelname = params.split(',')[0];
	var operName = params.split(',')[1];

	if (params == undefined || params == null || params.length == 0 || value.length == 0 || 
		propelname == undefined || propelname == null || propelname.length == 0 ||
		operName == undefined || operName == null || operName.length == 0) {
		return true;
	}	

	var valueOther = $(propelname).val();
	var val1 = (isNaN(value) ? Date.parse(value) : eval(value));
	var val2 = (isNaN(valueOther) ? Date.parse(valueOther) : eval(valueOther));

	if (operName == "GreaterThan")
		return val1 > val2;
	if (operName == "GreaterThanOrEqual")
		return val1 >= val2;
	if (operName == "LessThan")
		return val1 < val2;
	if (operName == "LessThanOrEqual")
		return val1 <= val2;
});
$.validator.unobtrusive.adapters.add("genericcompare", ["comparetopropertyname", "operatorname"], function (options) {
	options.rules["genericcompare"] = "#" + options.params.comparetopropertyname + "," + options.params.operatorname;
	options.messages["genericcompare"] = options.message;
});
