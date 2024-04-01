#pragma once
#ifndef VPK_UTILS_INCLUDE_VPK_APPLICATION_H_
#define VPK_UTILS_INCLUDE_VPK_APPLICATION_H_

#include <iostream>
#include <filesystem>
#include <algorithm>
#include <functional>
#include <chrono>
#include <ctime>

#include "vpk-utils/args.h"
#include "vpk-utils/utility.h"

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

class Application
{
public:
	struct About
	{
		static constexpr auto NAME = "vpk-utils";
		static constexpr auto VERSION = "0.2.1";
		static constexpr auto AUTHOR = "Nikita Milinkov";
		static constexpr auto LICENSE = "MIT Licence";

		friend auto operator<<(std::ostream& os, const About& a) -> std::ostream&
		{
			auto current_time = std::chrono::system_clock::to_time_t(std::chrono::system_clock::now());
			tm tminfo{ 0 };
			localtime_s(&tminfo, &current_time);
			int current_year = tminfo.tm_year + 1900;

			os << About::NAME
				<< " v" << About::VERSION << "  "
				<< About::LICENSE
				<< ".\nDeveloped by " << About::AUTHOR << " (" << current_year << ')';
			return os;
		}
	};

public:
	Application() = default;

public:
	inline auto SetWorkDir(std::string_view workdir) noexcept -> void
	{
		workdir_ = workdir;
	}

	inline auto SetSubDir(std::string_view subdir) noexcept -> void
	{
		subdir_ = subdir;
	}

	inline auto SetVerboseOutput(bool verbose) noexcept -> void
	{
		verbose_output_ = verbose;
	}

	inline auto SetMeasureExecTime(bool measure) noexcept -> void
	{
		measure_exec_time_ = measure;
	}

	inline auto GetFullPath() const noexcept -> std::string
	{
		return subdir_.has_value()
			? std::string(workdir_) + "\\" + std::string(subdir_.value())
			: std::string(workdir_);
	}

	inline auto Exec() -> int
	{
		Timer timer;
		std::cout << "Select directory:\n";
		auto dirs = ListAvailableDirectories_();

		size_t chosen_idx = utl::io::getint(1, dirs.size(), "Enter directory index: ");
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

private:
	inline auto ListAvailableDirectories_() -> std::vector<std::filesystem::directory_entry>
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

	inline auto ConstructNewFilename_(
		const std::string& old_name,
		const std::string& new_name,
		std::string_view suffix = "",
		std::string_view ext = ".jpg"
	) -> std::string
	{
		auto idx = utl::vpk::get_jpg_file_idx(old_name);
		return new_name + "_" + idx + suffix.data() + ext.data();
	}

	inline auto RenameAllFilesInDir_(
		const std::filesystem::path& dir,
		std::optional<std::string> name = {},
		std::string_view suffix = "",
		std::string_view file_ext = ".jpg"
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

private:
	std::string workdir_;
	std::optional<std::string_view> subdir_;
	bool verbose_output_{ false };
	bool measure_exec_time_{ false };
};

#endif  // VPK_UTILS_INCLUDE_VPK_APPLICATION_H_
