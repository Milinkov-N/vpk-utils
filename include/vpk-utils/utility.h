#pragma once
#ifndef VPK_UTILS_INCLUDE_VPK_UTILITY_H_
#define VPK_UTILS_INCLUDE_VPK_UTILITY_H_

#include <iostream>
#include <cstdlib>
#include <string>
#include <string_view>
#include <stdexcept>
#include <limits>
#include <filesystem>

namespace str
{
	constexpr auto starts_with(std::string_view str, std::string_view prefix) noexcept -> bool
	{
		return str.rfind(prefix, 0) == 0;
	}
}  // namespace str

namespace utl
{
	constexpr auto is_long_flag(std::string_view str) noexcept -> bool
	{
		return str::starts_with(str, "--");
	}

	constexpr auto is_short_flag(std::string_view str) noexcept -> bool
	{
		return str::starts_with(str, "-");
	}

	constexpr auto is_flag(std::string_view str) noexcept -> bool
	{
		return is_long_flag(str) || is_short_flag(str);
	}

	constexpr auto get_flag_name(const std::string_view& flag) noexcept -> std::optional<std::string_view>
	{
		if (is_long_flag(flag))
		{
			auto eq_sign = flag.find('=');
			return eq_sign != flag.npos
				? flag.substr(2, eq_sign - 2)
				: flag.substr(2);;
		}
		else if (is_short_flag(flag))
		{
			return flag.substr(1);
		}

		return {};
	}

	inline auto getenv(std::string_view var_name) -> std::string
	{
		std::string out;
		char* buf = nullptr;
		size_t buf_len = 0;

		if (_dupenv_s(&buf, &buf_len, var_name.data()) || buf == nullptr)
		{
			std::string err_msg;
			err_msg += var_name;
			err_msg += " environment variable is not set";
			throw std::runtime_error(err_msg);
		}

		out = buf;
		std::free(buf);
		return out;
	}

	namespace vpk
	{
		inline auto get_jpg_file_idx(std::string_view filename) -> std::string
		{
			auto space = filename.find(' ');
			auto sep = space == filename.npos ? filename.find('.') : space;
			return std::string(filename.substr(0, sep));
		}
	}  // 

	namespace io
	{
		inline auto getint(
			int lower_bound = std::numeric_limits<int>::min(),
			int upper_bound = std::numeric_limits<int>::max(),
			std::string_view prompt = ""
		) -> int
		{
			int tmp = 0;

			while (tmp < lower_bound || tmp > upper_bound)
			{
				std::cout << prompt << std::flush;
				std::cin >> tmp;
			}

			return tmp;
		}
	}  // namespace io
}  // namespace utl

#endif  // VPK_UTILS_INCLUDE_VPK_UTILITY_H_
