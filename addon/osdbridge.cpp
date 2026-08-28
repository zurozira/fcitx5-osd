#include <cstdlib>
#include <cstring>
#include <sstream>

#include <sys/socket.h>
#include <sys/un.h>
#include <unistd.h>

#include <fcitx-utils/log.h>
#include <fcitx/addonfactory.h>
#include <fcitx/addonmanager.h>
#include <fcitx/instance.h>
#include <fcitx/inputcontext.h>
#include <fcitx/inputmethodentry.h>
#include <fcitx/inputmethodmanager.h>

namespace fcitx {

namespace {
std::string jsonEscape(const std::string &input) {
    std::string out;
    for (char c : input) {
        if (c == '"' || c == '\\') out.push_back('\\');
        out.push_back(c);
    }
    return out;
}
}

class OsdBridge : public AddonInstance {
public:
    explicit OsdBridge(Instance *instance) : instance_(instance) {
       openSocket();
        handler_ = instance_->watchEvent(
            EventType::InputContextSwitchInputMethod, EventWatcherPhase::Default, [this](Event &event) {
                auto &icEvent = static_cast<InputContextEvent &>(event);
                notify(icEvent.inputContext());
            });
    }

    ~OsdBridge() override { if (sock_ >= 0) close(sock_); }

private:
    void openSocket() {
        sock_ = socket(AF_UNIX, SOCK_DGRAM, 0);
        std::memset(&addr_, 0, sizeof(addr_));
        addr_.sun_family = AF_UNIX;
        const char *runtime = std::getenv("XDG_RUNTIME_DIR");
        std::string path = (runtime && *runtime ? runtime : "/tmp");
        path += "/fcitx5-osd.sock";
        std::strncpy(addr_.sun_path, path.c_str(), sizeof(addr_.sun_path) - 1);
    }

    void notify(InputContext *ic) {
        if (sock_ < 0 || !ic) return;
        const auto &imName = instance_->inputMethod(ic);
        const auto *entry = instance_->inputMethodManager().entry(imName);
        if (!entry) return;

        std::ostringstream json;
        json << "{\"uniqueName\":\"" << jsonEscape(entry->uniqueName()) << "\","
             << "\"name\":\"" << jsonEscape(entry->name()) << "\","
             << "\"nativeName\":\"" << jsonEscape(entry->nativeName()) << "\"}\n";
        auto payload = json.str();
        sendto(sock_, payload.data(), payload.size(), 0,
               reinterpret_cast<sockaddr *>(&addr_), sizeof(addr_));
        FCITX_INFO() << "Sent OSD payload: " << payload;
    }

    Instance *instance_;
    std::unique_ptr<HandlerTableEntry<EventHandler>> handler_;
    int sock_ = -1;
    sockaddr_un addr_{};
};

class OsdBridgeFactory : public AddonFactory {
    AddonInstance *create(AddonManager *manager) override {
        return new OsdBridge(manager->instance());
    }
};

}

FCITX_ADDON_FACTORY(fcitx::OsdBridgeFactory)
