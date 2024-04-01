#pragma once
#ifndef VPK_UTILS_INCLUDE_VPK_APPLICATION_H_
#define VPK_UTILS_INCLUDE_VPK_APPLICATION_H_

#include <iostream>
#include <filesystem>
#include <algorithm>
#include <functional>
#include <chrono>
#include <ctime>

#include "vpk-utils/Args.h"
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
	auto ElapsedMilliseconds() const->int64_t;

private:
	TimePoint<SysClock> start_;
	TimePoint<SysClock> end_;
	bool running_{ false };
};

class Application
{
public:
	struct About
	{
		static constexpr auto NAME = "vpk-utils";
		static constexpr auto VERSION = "0.2.1";
		static constexpr auto AUTHOR = "Nikita Milinkov";
		static constexpr auto LICENSE = "MIT Licence";

		friend auto operator<<(std::ostream& os, const About& a) -> std::ostream&;
	};

public:
	Application() = default;

public:
	auto SetWorkDir(std::string_view workdir) noexcept -> void;
	auto SetSubDir(std::string_view subdir) noexcept -> void;
	auto SetVerboseOutput(bool verbose) noexcept -> void;
	auto SetMeasureExecTime(bool measure) noexcept -> void;
	auto GetFullPath() const noexcept -> std::string;
	auto Exec() -> int;

private:
	auto ListAvailableDirectories_() -> std::vector<std::filesystem::directory_entry>;

	auto ConstructNewFilename_(
		const std::string& old_name,
		const std::string& new_name,
		std::string_view suffix = "",
		std::string_view ext = ".jpg"
	) -> std::string;

	auto RenameAllFilesInDir_(
		const std::filesystem::path& dir,
		std::optional<std::string> name = {},
		std::string_view suffix = "",
		std::string_view file_ext = ".jpg"
	) -> size_t;

private:
	std::string workdir_;
	std::optional<std::string_view> subdir_;
	bool verbose_output_{ false };
	bool measure_exec_time_{ false };
};

#endif  // VPK_UTILS_INCLUDE_VPK_APPLICATION_H_
