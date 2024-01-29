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
		inline auto FindArg(FlagSpec flag_name) -> std::pair<bool, ArgValue>;

	private:
		inline auto ParseLongArg_(const FlagSpec& spec, std::string_view arg, int idx)
			-> std::pair<bool, ArgValue>;
		inline auto ParseShortArg_(const FlagSpec& spec, std::string_view arg, int idx)
			-> std::pair<bool, ArgValue>;
		inline auto ParseArgError_(std::string_view arg_name) -> std::logic_error;

	private:
		int argc_{ 0 };
		char** argv_{ nullptr };
	};

	Args::Args(int argc, char** argv) : argc_(argc), argv_(argv)
	{
		if (argc_ == 0 || argv_ == nullptr)
			throw std::runtime_error("argc or argv was null");
	}

	inline auto Args::FindArg(FlagSpec spec) -> std::pair<bool, ArgValue>
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

	inline auto Args::ParseLongArg_(const FlagSpec& spec, std::string_view arg, int idx)
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

	inline auto Args::ParseShortArg_(const FlagSpec& spec, std::string_view arg, int idx)
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

	inline auto Args::ParseArgError_(std::string_view arg_name) -> std::logic_error
	{
		std::string err_msg;
		err_msg += "no value was found while parsing argument \"";
		err_msg += arg_name;
		err_msg += "\"";
		return std::logic_error(err_msg);
	}
}  // namespace args

#endif  // VPK_UTILS_INCLUDE_VPK_ARGS_H_
