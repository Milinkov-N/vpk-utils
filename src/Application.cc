#include "vpk-utils/Application.h"

auto Application::SetWorkDir(std::string_view workdir) noexcept -> void
{
	workdir_ = workdir;
}

auto Application::SetSubDir(std::string_view subdir) noexcept -> void
{
	subdir_ = subdir;
}

auto Application::SetVerboseOutput(bool verbose) noexcept -> void
{
	verbose_output_ = verbose;
}

auto Application::SetMeasureExecTime(bool measure) noexcept -> void
{
	measure_exec_time_ = measure;
}

auto Application::GetFullPath() const noexcept -> std::string
{
	return subdir_.has_value()
		? std::string(workdir_) + "\\" + std::string(subdir_.value())
		: std::string(workdir_);
}

auto Application::Exec() -> int
{
	Timer timer;
	std::cout << "Select directory:\n";
	auto dirs = ListAvailableDirectories_();

	auto chosen_idx = utl::io::getint(1, (int)dirs.size(), "Enter directory index: ");
	const auto& chosen_dir_path = dirs[chosen_idx - 1].path();
	auto log_results = [&](std::string_view prompt, std::function<size_t()> cb)
		{
			std::cout << prompt << (verbose_output_ ? '\n' : '\t');
			auto n = cb();
			if (!verbose_output_) std::cout << n << " files renamed";
			std::cout << std::endl;
		};

	timer.Start();

	log_results("\\ JPG Files:", [&]
		{
			return RenameAllFilesInDir_(chosen_dir_path);
		});

	log_results("\\ PSD Files:", [&]
		{
			return RenameAllFilesInDir_(chosen_dir_path, {}, "", ".psd");
		});

	log_results("\\BP JPG Files:", [&]
		{
			return RenameAllFilesInDir_(chosen_dir_path / "BP", chosen_dir_path.filename().string(), " вр");
		});

	log_results("\\DM JPG Files:", [&]
		{
			return RenameAllFilesInDir_(chosen_dir_path / "DM", chosen_dir_path.filename().string(), " дм");
		});

	timer.Stop();

	if (measure_exec_time_)
		std::cout << "Finished in " << timer.ElapsedMilliseconds() << "ms\n";

	return 0;
}

auto Application::ListAvailableDirectories_()
	-> std::vector<std::filesystem::directory_entry>
{
	auto idx = 0;
	std::vector<std::filesystem::directory_entry> entries;
	auto dir_it = std::filesystem::directory_iterator(GetFullPath());

	std::for_each(dir_it, std::filesystem::directory_iterator(), [&](const auto& entry)
		{
			if (entry.is_directory())
			{
				std::cout << "\t[" << ++idx << "]\t" << entry.path().filename().string() << std::endl;
				entries.push_back(entry);
			}
		});

	return entries;
}

auto Application::ConstructNewFilename_(
	const std::string& old_name,
	const std::string& new_name,
	std::string_view suffix,
	std::string_view ext
) -> std::string
{
	auto idx = utl::vpk::get_jpg_file_idx(old_name);
	return new_name + "_" + idx + suffix.data() + ext.data();
}

auto Application::RenameAllFilesInDir_(
	const std::filesystem::path& dir,
	std::optional<std::string> name,
	std::string_view suffix,
	std::string_view file_ext
) -> size_t
{
	auto dir_it = std::filesystem::directory_iterator(dir);
	std::vector<std::filesystem::directory_entry> entries;

	for (const auto& entry : dir_it)
		if (entry.path().extension() == file_ext)
			entries.push_back(entry);

	std::for_each(entries.cbegin(), entries.cend(), [&](const auto& entry)
		{
			auto old_name = entry.path().filename();
			auto new_name = ConstructNewFilename_(
				entry.path().filename().string(),
				name.has_value() ? name.value() : dir.filename().string(),
				suffix,
				file_ext
			);
			auto old_path = dir / old_name;
			auto new_path = dir / new_name;

			if (!std::filesystem::exists(new_path))
				std::filesystem::rename(old_path, new_path);
			if (verbose_output_)
				std::cout << "\t" << old_name << "\t-->\t" << new_name << std::endl;
		});

	return entries.size();
}

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

auto Timer::ElapsedMilliseconds() const -> int64_t
{
	TimePoint<SysClock> end_time;

	if (running_)
		end_time = SysClock::now();
	else
		end_time = end_;

	return std::chrono::duration_cast<std::chrono::milliseconds>(end_time - start_).count();
}

auto operator<<(std::ostream& os, const Application::About& a) -> std::ostream&
{
	auto current_time = std::chrono::system_clock::to_time_t(std::chrono::system_clock::now());
	tm tminfo{ 0 };
	localtime_s(&tminfo, &current_time);
	int current_year = tminfo.tm_year + 1900;

	os << Application::About::NAME
		<< " v" << Application::About::VERSION << "  "
		<< Application::About::LICENSE
		<< ".\nDeveloped by " << Application::About::AUTHOR << " (" << current_year << ')';
	return os;
}
