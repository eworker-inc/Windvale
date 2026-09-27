import json
import resource
import subprocess
import sys
import time

runner, guest = sys.argv[1:3]
started = time.monotonic()
result = subprocess.run([runner, guest, "--report-steps"],
                        capture_output=True, text=True, timeout=45)
print(json.dumps({"runner": runner, "guest": guest,
                  "seconds": round(time.monotonic() - started, 3),
                  "peakRssKiB": resource.getrusage(resource.RUSAGE_CHILDREN).ru_maxrss,
                  "exit": result.returncode,
                  "stdout": result.stdout[:1024], "stderr": result.stderr[:1024]}))
