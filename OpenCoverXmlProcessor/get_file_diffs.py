import json
import os
import re
import subprocess

#os.system(r'cd D:\github\ZnYang2018\hello-world')
#os.chdir(r'D:\github\ZnYang2018\hello-world')
print(os.getcwd())
cmd = 'git diff d76d7ebe5fb93e718221627fa7885a07f530f4fd b8ed9117742ff8fde795e3e6af4b0112aeacf4ff'


def execute_cmd(command):
    process = subprocess.Popen(command, shell=True, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    lines = []
    while True:
        result = process.stdout.readline()
        print(result)
        if result:
            try:
                lines.append(result.decode('gbk').strip('\r\n'))
            except UnicodeDecodeError:
                lines.append(result.decode('utf-8').strip('\r\n'))
            except Exception as e:
                print(e)
        else:
            break
    return lines


class FileDiff(object):
    def __init__(self, lines):
        self.lines = lines

    def __repr__(self):
        return f'ChangedLines:{self.get_changed_lines()}, IsSource:{self.is_source_code()}, Path:{self.get_file_path()}'

    def get_file_path(self):
        """Get the changed file's path

        :return:
        """
        first_line = self.lines[0]
        path = re.search('diff --git a(/.*) b(/.*)', first_line, re.RegexFlag.I).groups()[1]
        return path

    def is_source_code(self):
        """Get if the changed file is source code file

        :return:
        """
        path = self.get_file_path()
        is_cs = os.path.splitext(os.path.basename(path))[1].lower() in ['.cs', '.py']
        return is_cs

    def get_changed_lines(self):
        changed_lines = []
        for line in self.lines:
            if line.startswith('@@'):
                changed_line = re.search('@@ -\d+,\d+ \+(\d+,\d+) @@', line, re.RegexFlag.I).groups()[0]
                changed_lines.append(tuple([int(item) for item in changed_line.split(',')]))
        return changed_lines

    def to_json_data_obj(self):
        return {'path': self.get_file_path(), 'lines': self.get_changed_lines()}


class FileDiffData(object):
    def __init__(self, path, lines):
        self.path = path
        self.lines = lines


def generate_file_diffs(lines):
    file_diffs = []
    file_lines = []
    for line in lines:
        if line.startswith('diff --git'):
            if file_lines:
                file_diffs.append(FileDiff(file_lines))
            file_lines = []
        file_lines.append(line)
    else:
        if file_lines:
            file_diffs.append(FileDiff(file_lines))
            file_lines = []
    return file_diffs


result_lines = execute_cmd(cmd)
file_diff_list = generate_file_diffs(result_lines)

data = {'diffs': [item.to_json_data_obj() for item in file_diff_list if item.is_source_code()]}
json_data = json.dumps(data)
print(json_data, file=open('data.json', 'w'))
# https://stackoverflow.com/questions/2529441/how-to-read-the-output-from-git-diff
