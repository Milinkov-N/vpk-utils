#include "vpk-utils/Args.h"

args::Args::Args(int argc, char** argv) : argc_(argc), argv_(argv)
{
	if (argc_ == 0 || argv_ == nullptr)
		throw std::runtime_error("argc or argv was null");
}

auto args::Args::FindArg(FlagSpec spec) -> std::pair<bool, ArgValue>
{
	for (int i = 1; i < argc_; i++)
	{
		std::string_view arg = argv_[i];
		auto name = utl::get_flag_name(arg);

		if (utl::is_long_flag(arg) && name.value() == spec.long_name())
			return ParseLongArg_(spec, arg, i);
		else if (str::starts_with(arg, "-")
			&& spec.short_name().has_value()
			&& name == spec.short_name().value())
			return ParseShortArg_(spec, arg, i);
	}

	return { false, {} };
}

auto args::Args::ParseLongArg_(const FlagSpec& spec, std::string_view arg, int idx)
-> std::pair<bool, ArgValue>
{
	if (spec.has_value())
	{
		auto eq_sign_pos = arg.find('=');
		if (eq_sign_pos == std::string_view::npos)
		{
			if (idx + 1 != argc_ && !utl::is_flag(argv_[idx + 1]))
				return { true, argv_[idx + 1] };
			else
				throw ParseArgError_(arg);
		}
		else if (arg.begin() + eq_sign_pos + 1 == arg.end())
		{
			throw ParseArgError_(arg);
		}

		return { true, arg.substr(eq_sign_pos + 1) };
	}

	return { true, {} };
}

auto args::Args::ParseShortArg_(const FlagSpec& spec, std::string_view arg, int idx)
-> std::pair<bool, ArgValue>
{
	if (spec.has_value())
	{
		if (idx + 1 != argc_ && !utl::is_flag(argv_[idx + 1]))
			return { true, argv_[idx + 1] };

		throw ParseArgError_(arg);
	}

	return { true, {} };
}

auto args::Args::ParseArgError_(std::string_view arg_name) -> std::logic_error
{
	std::string err_msg;
	err_msg += "no value was found while parsing argument \"";
	err_msg += arg_name;
	err_msg += "\"";
	return std::logic_error(err_msg);
}
