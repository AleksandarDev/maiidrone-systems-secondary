import redis
import time
import threading
from DependencyManager import DependencyManager

# Check and install missing packages
dependenciesResult: int = DependencyManager().check_dependencies([
    # ['import', (optional)'pip package name']
    ['serial', 'pyserial'],
    ['inject'],
    ['redis']
])

# Exit the app if package dependency check failed
if dependenciesResult:
    print("Some packages failed to install. Install them manually and try again.")
    exit(1)


class BrainContext:

    __reconnect_delay_s = 1

    def __init__(self):
        self.redis_host = "localhost"
        self.redis_port = 6379

    def init_connection(self):
        self.__connect()

    def __connect(self):
        print("Connecting to Redis on " + self.redis_host + ":" + str(self.redis_port), end='\t', flush=True)
        try:
            redis_db = redis.StrictRedis(host=self.redis_host, port=self.redis_port, db=0)
            redis_ping_result = redis_db.ping()
            if redis_ping_result is False:
                raise Exception("Redis didn't respond to ping.")
            print("[OK]")
        except:
            print("[FAILED]")

            # Delay and reconnect
            time.sleep(self.__reconnect_delay_s)
            self.__connect()


BrainContext().init_connection()