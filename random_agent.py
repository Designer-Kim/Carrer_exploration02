from mlagents_envs.environment import UnityEnvironment

if __name__ == '__main__':
    # 환경을 정의
    env = UnityEnvironment(file_name='./Env/3DBall/3DBall')

    # behavior 불러오기
    env.reset()
    behavior_name = list(env.behavior_specs.keys())[0]
    print(f'name of behavior: {behavior_name}')
    spec = env.behavior_specs[behavior_name]

    # 에피소드 진행을 위한 반복문 (10 에피소드 반복)
    for ep in range(10):
        # 환경 초기화
        env.reset()

        # 에이전트가 행동을 요청한 상태인지, 마지막 상태인지 확인
        decision_steps, terminal_steps = env.get_steps(behavior_name)

        # 한 에이전트를 기준으로 로그를 출력
        tracked_agent = -1
        done = False
        ep_rewards = 0

        # 에이전트가 행동을 완료할 때까지 반복
        while not done:
            # tracked agent 지정
            if tracked_agent == -1 and len(decision_steps) >= 1:
                tracked_agent = decision_steps.agent_id[0]

            # 랜덤 액션 결정
            action = spec.action_spec.random_action(len(decision_steps))

            # set actions
            env.set_actions(behavior_name, action)

            # 실제 액션 수행
            env.step()

            # 스텝 종료 후 에이전트의 정보 (보상, 상태 등) 취득
            decision_steps, terminal_steps = env.get_steps(behavior_name)

            # 추적중인 에이전트가 행동이 가능한 상태와 종료 상태일 때를 구분하여 보상 저장
            if tracked_agent in decision_steps:
                ep_rewards += decision_steps[tracked_agent].reward
            if tracked_agent in terminal_steps:
                ep_rewards += terminal_steps[tracked_agent].reward
                done = True

        # 한 에피소드가 종료되고 추적중인 에이전트에 대해서 해당 에피소드에서의 보상 출력
        print(f'total reward for ep {ep} is {ep_rewards}')

    # 환경 종료
    env.close()
