#include <windows.h>
#include <ctype.h>
#include <tchar.h>
#include <stdio.h>
#include <stdlib.h>
#include <strsafe.h>
#include <locale.h>

#pragma comment(lib, "User32.lib")

typedef WIN32_FIND_DATA find_data_t;
typedef LARGE_INTEGER largeint_t;

typedef struct rename_jpg_state
{
	wchar_t* dirpath;
	wchar_t* env_path;
	wchar_t* subdir;
	wchar_t* suffix;
	size_t env_path_sz;
	size_t subdir_sz;
} rename_jpg_state_t;

typedef enum list_dir_type
{
	LIST_DIR_FILE,
	LIST_DIR_DIRECTORY,
} list_dir_type_t;

typedef struct wstr_vec
{
	size_t size;
	size_t capacity;
	wchar_t** entries;
} wstr_vec_t;

int wstr_vec_init(wstr_vec_t* out);
void wstr_vec_push(wstr_vec_t* self, wchar_t* entry);
void wstr_vec_destroy(wstr_vec_t* self);
int list_dir_files(wstr_vec_t* self, wchar_t* dirpath);
int list_dir_directories(wstr_vec_t* self, wchar_t* dirpath);
int _list_dir_inner_impl(wstr_vec_t* self, wchar_t* dirpath, list_dir_type_t type);

int get_no_of_jpg_src(wchar_t* jpg_name);
int get_renamed_jpg_src(wchar_t* oldname, wchar_t* new_name, wchar_t* suffix, wchar_t** out);

void rename_all_jpg_in_dir(rename_jpg_state_t state);

wchar_t* last_error_msg();

int _tmain(int argc, wchar_t** argv)
{
	wstr_vec_t dir_files = { 0 };
	size_t path_env_sz = 0;
	wchar_t* path_env = NULL;
	wchar_t dirpath[MAX_PATH] = { 0 };
	size_t subdir_sz = 0;

	setlocale(LC_ALL, "RU-ru");

	if (_wdupenv_s(&path_env, &path_env_sz, L"VPK_DIR") || path_env == NULL)
	{
		printf("Error: VPK_DIR Enviroment Variable is not set");
		return -1;
	}

	if (argc != 2)
	{
		printf("Usage: vpk-utils  <dirname>\n");
		return -1;
	}

	(void)StringCchLengthW(argv[1], MAX_PATH, &subdir_sz);

	if (subdir_sz + path_env_sz > (MAX_PATH - 4))
	{
		printf("Error: Directory path too long.\n");
		return -1;
	}

	swprintf_s(dirpath, MAX_PATH, L"%s\\%s", path_env, argv[1]);

	wprintf(L"Target directory is %s\n\n", dirpath);

	wprintf(L"/ JPG sources:\n");
	rename_all_jpg_in_dir((rename_jpg_state_t)
		{
			.dirpath      = dirpath,
			.env_path     = path_env,
			.subdir       = argv[1],
			.suffix = NULL,
			.env_path_sz  = path_env_sz,
			.subdir_sz    = subdir_sz
		});

	wchar_t bp_dirpath[MAX_PATH] = { 0 };
	swprintf_s(bp_dirpath, MAX_PATH, L"%s\\%s\\BP", path_env, argv[1]);

	wprintf(L"BP JPG sources:\n");
	rename_all_jpg_in_dir((rename_jpg_state_t)
		{
			.dirpath = bp_dirpath,
			.env_path = path_env,
			.subdir = argv[1],
			.suffix = L"_вр",
			.env_path_sz = path_env_sz,
			.subdir_sz = subdir_sz
		});

	wchar_t dm_dirpath[MAX_PATH] = { 0 };
	swprintf_s(dm_dirpath, MAX_PATH, L"%s\\%s\\DM", path_env, argv[1]);

	wprintf(L"DM JPG sources:\n");
	rename_all_jpg_in_dir((rename_jpg_state_t)
		{
			.dirpath = dm_dirpath,
			.env_path = path_env,
			.subdir = argv[1],
			.suffix = L"_дм",
			.env_path_sz = path_env_sz,
			.subdir_sz = subdir_sz
		});

	free(path_env);
	return 0;
}

int wstr_vec_init(wstr_vec_t* out)
{
	size_t capacity = 15;
	wchar_t** tmp = NULL;
	if (out == NULL) return -1;

	memset(out, 0, sizeof(wstr_vec_t));

	if ((tmp = (wchar_t**)malloc(capacity * sizeof(wchar_t*))) != NULL)
	{
		out->capacity = capacity;
		out->entries = tmp;
		return 0;
	}

	return -1;
}

void wstr_vec_push(wstr_vec_t* self, wchar_t* entry)
{
	if (self == NULL || entry == NULL) return;

	if (self->size + 1 == self->capacity)
	{
		size_t new_capacity = self->capacity * 2;
		wchar_t** tmp = NULL;

		if ((tmp = (wchar_t**)malloc(new_capacity * sizeof(wchar_t*))) != NULL)
		{
			for (size_t i = 0; i < self->size; i++) tmp[i] = self->entries[i];
			free(self->entries);
			self->capacity = new_capacity;
			self->entries = tmp;
		}
	}

	self->entries[self->size++] = entry;
}

void wstr_vec_destroy(wstr_vec_t* self)
{
	if (self == NULL) return;

	for (size_t i = 0; i < self->size; i++) free(self->entries[i]);
	free(self->entries);
	memset(self, 0, sizeof(wstr_vec_t));
}

int list_dir_files(wstr_vec_t* self, wchar_t* dirpath)
{
	return _list_dir_inner_impl(self, dirpath, LIST_DIR_FILE);
}

int list_dir_directories(wstr_vec_t* self, wchar_t* dirpath)
{
	return _list_dir_inner_impl(self, dirpath, LIST_DIR_DIRECTORY);
}

int _list_dir_inner_impl(wstr_vec_t* self, wchar_t* dirpath, list_dir_type_t type)
{
	if (self == NULL || dirpath == NULL) return;

	find_data_t ffd = { 0 };
	largeint_t file_sz = { 0 };
	HANDLE h_find = INVALID_HANDLE_VALUE;

	h_find = FindFirstFileW(dirpath, &ffd);

	if (h_find == INVALID_HANDLE_VALUE) return -1;

	do
	{
		wchar_t* entry = NULL;
		size_t entry_len = 0;

		switch (type)
		{
		case LIST_DIR_FILE:
		{
			(void)StringCchLengthW(ffd.cFileName, MAX_PATH, &entry_len);
			entry = (wchar_t*)malloc(entry_len * sizeof(wchar_t) + 1);

			if (entry != NULL)
			{
				(void)StringCchCopyW(entry, entry_len + 1, ffd.cFileName);
				wstr_vec_push(self, entry);
			}
			break;
		}
		case LIST_DIR_DIRECTORY:

		{
			if (ffd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
			{
				(void)StringCchLengthW(ffd.cFileName, MAX_PATH, &entry_len);
				entry = (wchar_t*)malloc(entry_len * sizeof(wchar_t) + 1);

				if (entry != NULL)
				{
					(void)StringCchCopyW(entry, entry_len + 1, ffd.cFileName);
					wstr_vec_push(self, entry);
				}
			}
			break;
		}
		}
	} while (FindNextFileW(h_find, &ffd) != 0);

	return 0;
}

int get_no_of_jpg_src(wchar_t* jpg_name)
{
	wchar_t no[4] = { 0 };
	wchar_t* space = wcschr(jpg_name, L' ');
	if (space != NULL)
	{
		int is_digit = TRUE;

		*space = L'\0';
		StringCchCopyW(no, 4, jpg_name);
		*space = L' ';

		for (wchar_t* ptr = no; *ptr && is_digit != FALSE; ++ptr)
			is_digit = iswdigit(*ptr);

		if (!is_digit) return -1;
		return wcstol(no, NULL, 10);
	}
	return -1;
}

int get_renamed_jpg_src(wchar_t* oldname, wchar_t* new_name, wchar_t* suffix, wchar_t** out)
{
	wchar_t* renamed_jpg = NULL;
	size_t renamed_jpg_sz = 0;
	size_t new_name_sz = 0;
	size_t suffix_sz = 0;
	int n = 0;

	if (oldname == NULL || new_name == NULL || out == NULL) return -1;

	(void)StringCchLengthW(new_name, MAX_PATH, &new_name_sz);
	if (new_name_sz > MAX_PATH) return -1;

	if (suffix != NULL)
	{
		(void)StringCchLengthW(suffix, MAX_PATH, &suffix_sz);
		if (suffix_sz > MAX_PATH) return -1;
	}

	renamed_jpg_sz = new_name_sz + suffix_sz + sizeof(".jpg") + 4;
	if (renamed_jpg_sz > MAX_PATH) return -1;

	n = get_no_of_jpg_src(oldname);
	// wprintf(L"\t%s\t -->\t", oldname);
	renamed_jpg = (wchar_t*)malloc(renamed_jpg_sz * sizeof(wchar_t));
	if (renamed_jpg == NULL) return -1;

	swprintf_s(
		renamed_jpg,
		renamed_jpg_sz,
		L"%s %d%s%s",
		new_name,
		n,
		suffix == NULL ? L"" : suffix,
		L".jpg"
	);
	*out = renamed_jpg;
	// wprintf(L"%s\n", renamed_jpg);
	return 0;
}

void rename_all_jpg_in_dir(rename_jpg_state_t state)
{
	wchar_t dirpath[MAX_PATH] = { 0 };
	wstr_vec_t dir_files = { 0 };
	wstr_vec_init(&dir_files);

	swprintf_s(dirpath, MAX_PATH, L"%s\\*", state.dirpath);

	if (list_dir_files(&dir_files, dirpath) == 0)
	{
		for (size_t i = 0; i < dir_files.size; i++)
		{
			wchar_t* file = dir_files.entries[i];
			wchar_t* ext = wcschr(file, L'.');
			if (ext != NULL && !wcscmp(ext + 1, L"jpg"))
			{
				wchar_t* renamed_jpg = NULL;
				size_t renamed_jpg_sz = 0;

				if (get_renamed_jpg_src(file, state.subdir, state.suffix, &renamed_jpg))
				{
					wprintf(L"Error: failed to generate new name for file \"%s\"", file);
					continue;
				}

				(void)StringCchLengthW(renamed_jpg, MAX_PATH, &renamed_jpg_sz);

				if (state.env_path_sz + state.subdir_sz + renamed_jpg_sz + 2 > MAX_PATH)
				{
					wprintf(L"Error: new filepath is too long");
				}
				else
				{
					wchar_t full_old_path[MAX_PATH] = { 0 };
					wchar_t full_renamed_path[MAX_PATH] = { 0 };

					swprintf_s(
						full_old_path,
						MAX_PATH,
						L"%s\\%s",
						state.dirpath,
						file
					);

					swprintf_s(
						full_renamed_path,
						MAX_PATH,
						L"%s\\%s",
						state.dirpath,
						renamed_jpg
					);

					if (!_wrename(full_old_path, full_renamed_path))
					{
						wprintf(L"\t%s\t -->\t", file);
						wprintf(L"%s\n", renamed_jpg);
						wprintf(L"New Full Path: %s\n", full_renamed_path);
					}
				}

				free(renamed_jpg);
			}
		}
	}
	else
	{
		wchar_t* err_msg = last_error_msg();
		wprintf(L"Error: %s", err_msg);
		LocalFree(err_msg);
	}
}

wchar_t* last_error_msg()
{
	void* msg_buf = NULL;
	void* display_buf = NULL;
	DWORD dw = GetLastError();

	FormatMessageW(
		FORMAT_MESSAGE_ALLOCATE_BUFFER
			| FORMAT_MESSAGE_FROM_SYSTEM
			| FORMAT_MESSAGE_IGNORE_INSERTS,
		NULL,
		dw,
		MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
		(wchar_t*)&msg_buf,
		0,
		NULL
	);

	return msg_buf;
}
