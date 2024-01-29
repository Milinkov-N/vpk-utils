#include <iostream>
#include <clocale>
#include <sstream>

#include "vpk-utils/args.h"
#include "vpk-utils/utility.h"
#include "vpk-utils/Application.h"


class Timer
{
public:
	using SysClock = std::chrono::system_clock;
	template<class T>
	using TimePoint = std::chrono::time_point<T>;

public:
	auto Start() -> void;
	auto Stop() -> void;
	auto ElapsedMilliseconds() -> int64_t;
	auto TimeMillis(std::function<void()> callback) -> int64_t;

private:
	TimePoint<SysClock> start_;
	TimePoint<SysClock> end_;
	bool running_{ false };
};

auto Timer::Start() -> void
{
	start_ = SysClock::now();
	running_ = true;
}

auto Timer::Stop() -> void
{
	end_ = SysClock::now();
	running_ = false;
}

auto Timer::ElapsedMilliseconds() -> int64_t
{
	TimePoint<SysClock> end_time;

	if (running_)
		end_time = SysClock::now();
	else
		end_time = end_;

	return std::chrono::duration_cast<std::chrono::milliseconds>(end_time - start_).count();
}

auto Timer::TimeMillis(std::function<void()> callback) -> int64_t
{
	Start();
	callback();
	Stop();
	return ElapsedMilliseconds();
}

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
	constexpr auto EXEC_TIME = args::FlagSpec("exec-time", "t");
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
		auto [time_exec_isset, _te] = args.FindArg(flags::EXEC_TIME);
		auto [help_isset, _h] = args.FindArg(flags::HELP);

		if (help_isset || argc < 2)
		{
			Usage usage = { flags::WORKDIR, flags::SUBDIR, flags::VERBOSE, flags::HELP };
			
			std::cout << Application::about() << "\n\n";
			std::cout << usage;
			return 0;
		}

		app.SetVerboseOutput(verbose_isset);

		if (workdir_isset) app.SetWorkDir(workdir.value());
		else app.SetWorkDir(utl::getenv("VPK_DIR"));

		if (subdir_isset) app.SetSubDir(subdir.value());

		Timer timer;
		int status = 0;
		auto millis = timer.TimeMillis([&] { status = app.Exec(); });
		if (time_exec_isset) std::cout << "Finished in " << millis << "ms\n";

		return status;
	}
	catch (const std::exception& e)
	{
		std::cerr << "fatal error: " << e.what() << std::endl;
		return -1;
	}

	return 0;
}
