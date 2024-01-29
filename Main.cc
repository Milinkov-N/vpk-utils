#include <iostream>
#include <clocale>
#include <sstream>

#include "vpk-utils/args.h"
#include "vpk-utils/utility.h"
#include "vpk-utils/Application.h"

class Usage
{
public:
	Usage(std::initializer_list <args::FlagSpec> flag_specs_)
	{
		msg_ << "USAGE:\n";
		for (const auto& spec : flag_specs_)
		{
			msg_  << std::setw(8) << "--" << spec.long_name();

			if (spec.short_name().has_value())
				msg_ << ", -" << spec.short_name().value();
			msg_ << "\t" << "Default Description\n\n";
		}
	}

public:
	friend auto operator<<(std::ostream& os, const Usage& usage) -> std::ostream&
	{
		os << usage.msg_.str();
		return os;
	}

private:
	std::stringstream msg_;
};

namespace flags
{
	constexpr auto WORKDIR   = args::FlagSpec("workdir", "w", true);
	constexpr auto SUBDIR    = args::FlagSpec("subdir", "s", true);
	constexpr auto VERBOSE   = args::FlagSpec("verbose", "v");
	constexpr auto HELP      = args::FlagSpec("help", "h");
}  // namespace flags

auto main(int argc, char** argv) -> int
{
	Application app;
	args::Args args(argc, argv);

	std::setlocale(LC_ALL, "Ru-ru");

	try
	{
		auto [workdir_isset, workdir] = args.FindArg(flags::WORKDIR);
		auto [subdir_isset, subdir] = args.FindArg(flags::SUBDIR);
		auto [verbose_isset, _] = args.FindArg(flags::VERBOSE);
		auto [help_isset, __] = args.FindArg(flags::HELP);

		if (help_isset)
		{
			Usage usage = {
				flags::WORKDIR,
				flags::SUBDIR,
				flags::VERBOSE,
				flags::HELP
			};
			
			std::cout << Application::about() << "\n\n";
			std::cout << usage;
			return 0;
		}

		app.SetVerboseOutput(verbose_isset);

		if (workdir_isset) app.SetWorkDir(workdir.value());
		else app.SetWorkDir(utl::getenv("VPK_DIR"));

		if (subdir_isset) app.SetSubDir(subdir.value());

		return app.Exec();
	}
	catch (const std::exception& e)
	{
		std::cerr << "fatal error: " << e.what() << std::endl;
		return -1;
	}

	return 0;
}
