import pip


class DependencyManager:
    """
    The application dependency manager.
    Call check_dependencies to ensure all required dependencies are installed.
    If dependency import fails, pip install will be executed.
    """

    def __init__(self):
        """
        Initializes the DependencyManager class.
        """
        self.dependencies = []
        self.failed_dependencies = []
        self.installed_dependencies = []
        self.__print_spacing = 1

    def check_dependencies(self, dependencies: list) -> int:
        """
        Checks whether all specified dependencies are available to import.

        :type dependencies: list
        :param dependencies: The collection of package names ['import', (optional) 'pip package name']
        :return: Returns the number of failed dependencies.
                 You can check which dependencies failed using failed_dependencies.
                 You can check which dependencies were installed using installed_dependencies.
        """
        self.dependencies = dependencies
        self.failed_dependencies = []
        self.__print_spacing = 1

        # First determine max package name length
        self.__determine_print_spacing()

        # Check all dependencies
        print("Checking dependencies... (", len(self.dependencies), ")")
        for dep in self.dependencies:
            try:
                # Import the dependency
                __import__(DependencyManager.__get_dep_import_name(dep))
                self.__print_package_with_ok_result(DependencyManager.__get_dep_pip_name(dep))
            except ImportError:
                # Try to install dependency using pip
                self.__print_package_with_installing_result(DependencyManager.__get_dep_pip_name(dep))
                pip_result = pip.main(["install", DependencyManager.__get_dep_pip_name(dep), "--quiet"])

                # Check whether pip installation failed or succeeded
                if not pip_result:
                    self.installed_dependencies.append(dep)
                    DependencyManager.__print_reset_line()
                    self.__print_package_with_ok_result(DependencyManager.__get_dep_pip_name(dep))
                else:
                    self.failed_dependencies.append(dep)
                    DependencyManager.__print_reset_line()
                    self.__print_package_with_failed_result(DependencyManager.__get_dep_pip_name(dep))

        # Return result (number of failed dependencies)
        return len(self.failed_dependencies)

    def __determine_print_spacing(self):
        for dep in self.dependencies:
            self.__print_spacing = max(len(DependencyManager.__get_dep_pip_name(dep)), self.__print_spacing)

    def __print_package_with_result(self, package_name: str, result: str, is_end: bool = False):
        end_value = '\n\r' if is_end is True else ''
        print("\t", package_name.ljust(self.__print_spacing), "\t[", result, "]", end=end_value, flush=True)

    def __print_package_with_ok_result(self, package_name: str):
        self.__print_package_with_result(package_name, "OK", True)

    def __print_package_with_installing_result(self, package_name: str):
        self.__print_package_with_result(package_name, "Installing...")

    def __print_package_with_failed_result(self, package_name: str):
        self.__print_package_with_result(package_name, "Failed", True)

    @staticmethod
    def __get_dep_pip_name(dep: list):
        return dep[0] if len(dep) is 1 else dep[1]

    @staticmethod
    def __get_dep_import_name(dep: list):
        return dep[0]

    @staticmethod
    def __print_reset_line():
        print("\r", end='', flush=True)