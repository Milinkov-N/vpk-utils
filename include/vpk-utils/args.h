#pragma once
#ifndef VPK_UTILS_INCLUDE_VPK_ARGS_H_
#define VPK_UTILS_INCLUDE_VPK_ARGS_H_

#include <string_view>
#include <stdexcept>
#include <optional>
#include <vector>
#include <utility>

#include "vpk-utils/utility.h"

namespace args
{
	class FlagSpec
	{
	public:
		using LongName = std::string_view;
		using ShortName = std::optional<std::string_view>;

	public:
		constexpr FlagSpec(
			LongName long_name,
			ShortName short_name = {},
			bool has_value = false
		);

	public:
		constexpr auto long_name() const noexcept -> std::string_view;
		constexpr auto short_name() const noexcept -> ShortName;
		constexpr auto has_value() const noexcept -> bool;

	private:
		LongName long_;
		ShortName short_;
		bool has_value_{ false };
	};

	constexpr FlagSpec::FlagSpec(
		LongName long_name,
		ShortName short_name,
		bool has_value
	)	: long_(long_name)
		, short_(short_name)
		, has_value_(has_value) {};

	constexpr auto FlagSpec::long_name() const noexcept -> std::string_view
	{
		return long_;
	}

	constexpr auto FlagSpec::short_name() const noexcept -> ShortName
	{
		return short_;
	}

	constexpr auto FlagSpec::has_value() const noexcept -> bool
	{
		return has_value_;
	}

	class Args
	{
	public:
		using ArgValue = std::optional<std::string_view>;

	public:
		Args() = delete;
		Args(int argc, char** argv);

	public:
		auto FindArg(FlagSpec flag_name) -> std::pair<bool, ArgValue>;

	private:
		auto ParseLongArg_(const FlagSpec& spec, std::string_view arg, int idx)
			-> std::pair<bool, ArgValue>;
		auto ParseShortArg_(const FlagSpec& spec, std::string_view arg, int idx)
			-> std::pair<bool, ArgValue>;
		auto ParseArgError_(std::string_view arg_name) -> std::logic_error;

	private:
		int argc_{ 0 };
		char** argv_{ nullptr };
	};
}  // namespace args

#endif  // VPK_UTILS_INCLUDE_VPK_ARGS_H_
